using BookLoan.API;
using BookLoan.API.DomainModel;

namespace BookLoan.Tests;

public class BookLoanServiceTests
{
    [Test]
    public void BorrowBook_WhenBookIsAvailable_SetsBorrower()
    {
        var book = new Book { Id = 1, Title = "The Hobbit" };

        var repository = new FakeBookRepository(book);

        var service = new BookLoanService(repository);


        service.BorrowBook(1, "Grace");


        Assert.That(book.BorrowedBy, Is.EqualTo("Grace"));
    }

    [Test]
    public void BorrowBook_WhenBookIsAvailable_SavesThroughRepository()
    {
        var book = new Book { Id = 1, Title = "The Hobbit" };

        var repository = new FakeBookRepository(book);

        var service = new BookLoanService(repository);


        service.BorrowBook(1, "Grace");


        Assert.That(repository.UpdateCount, Is.EqualTo(1));
    }

    [Test]
    public void BorrowBook_WhenBookIsAlreadyBorrowed_Fails()
    {
        var book = new Book { Id = 1, Title = "The Hobbit", BorrowedBy = "Ada" };

        var repository = new FakeBookRepository(book);

        var service = new BookLoanService(repository);


        var exception = Assert.Throws<InvalidOperationException>(
            () => service.BorrowBook(1, "Grace"));


        Assert.That(exception!.Message, Is.EqualTo("Boka er allerede utlånt."));

        Assert.That(book.BorrowedBy, Is.EqualTo("Ada"));
    }

    [Test]
    public void BorrowBook_WhenBookDoesNotExist_Fails()
    {
        var repository = new FakeBookRepository();

        var service = new BookLoanService(repository);


        var exception = Assert.Throws<InvalidOperationException>(
            () => service.BorrowBook(99, "Grace"));


        Assert.That(exception!.Message, Is.EqualTo("Boka finnes ikke."));
    }

    [Test]
    public void BorrowBook_WithoutUserName_Fails()
    {
        var book = new Book { Id = 1, Title = "The Hobbit" };

        var repository = new FakeBookRepository(book);

        var service = new BookLoanService(repository);


        Assert.Throws<ArgumentException>(
            () => service.BorrowBook(1, "   "));

        Assert.That(repository.UpdateCount, Is.EqualTo(0));
    }

    [Test]
    public void ReturnBook_WhenBorrowedByUser_ClearsBorrower()
    {
        var book = new Book { Id = 1, Title = "The Hobbit", BorrowedBy = "Grace" };

        var repository = new FakeBookRepository(book);

        var service = new BookLoanService(repository);


        service.ReturnBook(1, "Grace");


        Assert.That(book.BorrowedBy, Is.Null);
    }

    [Test]
    public void ReturnBook_WhenBorrowedBySomeoneElse_Fails()
    {
        var book = new Book { Id = 1, Title = "The Hobbit", BorrowedBy = "Ada" };

        var repository = new FakeBookRepository(book);

        var service = new BookLoanService(repository);


        var exception = Assert.Throws<InvalidOperationException>(
            () => service.ReturnBook(1, "Grace"));


        Assert.That(
            exception!.Message,
            Is.EqualTo("Boka er lånt ut til noen andre."));

        Assert.That(book.BorrowedBy, Is.EqualTo("Ada"));
    }

    [Test]
    public void ReturnBook_WhenBookIsNotBorrowed_Fails()
    {
        var book = new Book { Id = 1, Title = "The Hobbit" };

        var repository = new FakeBookRepository(book);

        var service = new BookLoanService(repository);


        var exception = Assert.Throws<InvalidOperationException>(
            () => service.ReturnBook(1, "Grace"));


        Assert.That(exception!.Message, Is.EqualTo("Boka er ikke utlånt."));
    }
}
