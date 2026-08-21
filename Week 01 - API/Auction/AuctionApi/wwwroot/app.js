const auctionsElement = document.getElementById('auctions');
const messageElement = document.getElementById('message');

async function loadAuctions() {
    const response = await fetch('/auctions');
    const auctions = await response.json();

    renderAuctions(auctions);
}

function renderAuctions(auctions) {
    const activeId = auctionsElement.contains(document.activeElement)
        ? document.activeElement.id
        : null;

    const typed = {};

    for (const input of auctionsElement.querySelectorAll('input')) {
        if (input.value !== '') {
            typed[input.id] = input.value;
        }
    }

    auctionsElement.innerHTML = '';

    for (const auction of auctions) {
        const div = document.createElement('div');

        div.className =
            auction.isClosed
                ? 'auction closed'
                : 'auction';

        div.innerHTML = `
            <h3>${auction.itemName}</h3>

            <p>
                Highest bid:
                <strong>${auction.currentBid} kr</strong>
            </p>

            <p>
                Highest bidder:
                <strong>
                    ${auction.highestBidder ?? 'Nobody yet'}
                </strong>
            </p>

            ${
                auction.isClosed
                    ? '<p><strong>This auction is closed</strong></p>'
                    : `
                        <div class="bid">
                            <input
                                id="amount-${auction.id}"
                                type="number"
                                placeholder="Your bid">

                            <button
                                onclick="placeBid(${auction.id})">
                                Place bid
                            </button>

                            <button
                                onclick="closeAuction(${auction.id})">
                                Close
                            </button>
                        </div>
                    `
            }
        `;

        auctionsElement.appendChild(div);
    }

    for (const id in typed) {
        const input = document.getElementById(id);

        if (input) {
            input.value = typed[id];
        }
    }

    if (activeId) {
        document.getElementById(activeId)?.focus();
    }
}

async function placeBid(auctionId) {
    clearMessage();

    const bidderName =
        document.getElementById('bidderName').value;

    const amount =
        Number(
            document.getElementById(
                `amount-${auctionId}`
            ).value
        );

    const response = await fetch(
        `/auctions/${auctionId}/bids`,
        {
            method: 'POST',

            headers: {
                'Content-Type': 'application/json'
            },

            body: JSON.stringify({
                bidderName: bidderName,
                amount: amount
            })
        });

    if (!response.ok) {
        const message = await response.text();
        showError(message);
        return;
    }

    showSuccess('Your bid was registered.');

    await loadAuctions();
}

async function closeAuction(auctionId) {
    clearMessage();

    const response = await fetch(
        `/auctions/${auctionId}`,
        {
            method: 'PATCH',

            headers: {
                'Content-Type': 'application/json'
            },

            body: JSON.stringify({
                isClosed: true
            })
        });

    if (!response.ok) {
        const message = await response.text();
        showError(message);
        return;
    }

    await loadAuctions();
}

document
    .getElementById('createAuctionForm')
    .addEventListener('submit', async event => {
        event.preventDefault();

        clearMessage();

        const itemName =
            document.getElementById('itemName').value;

        const startingPrice =
            Number(
                document.getElementById(
                    'startingPrice'
                ).value
            );

        const response =
            await fetch('/auctions', {
                method: 'POST',

                headers: {
                    'Content-Type': 'application/json'
                },

                body: JSON.stringify({
                    itemName: itemName,
                    startingPrice: startingPrice
                })
            });

        if (!response.ok) {
            const message = await response.text();
            showError(message);
            return;
        }

        event.target.reset();

        await loadAuctions();
    });

function showError(message) {
    messageElement.className = 'error';
    messageElement.textContent = message;
}

function showSuccess(message) {
    messageElement.className = 'success';
    messageElement.textContent = message;
}

function clearMessage() {
    messageElement.className = '';
    messageElement.textContent = '';
}

loadAuctions();

setInterval(() => {
    if (!document.hidden) {
        loadAuctions();
    }
}, 5000);
