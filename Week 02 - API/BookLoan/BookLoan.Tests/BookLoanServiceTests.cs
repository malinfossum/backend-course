using BookLoan.API;
using BookLoan.API.DomainModel;

namespace BookLoan.Tests;

public class BookLoanServiceTests
{
    [Test]
    public void BorrowBook_WhenBookIsAvailable_SetsBorrower()
    {
        var repository = new FakeBookRepository(
            new Book { Id = 1, Title = "The Hobbit" });

        var service = new BookLoanService(repository);


        service.BorrowBook(1, "Grace");


        // Read back through the repository, not from the local variable.
        // The fake hands out copies, so this only passes if the service
        // actually wrote the change back.
        Assert.That(repository.Get(1)!.BorrowedBy, Is.EqualTo("Grace"));
    }

    [Test]
    public void BorrowBook_WhenBookIsAvailable_SavesThroughRepository()
    {
        var repository = new FakeBookRepository(
            new Book { Id = 1, Title = "The Hobbit" });

        var service = new BookLoanService(repository);


        service.BorrowBook(1, "Grace");


        Assert.That(repository.UpdateCount, Is.EqualTo(1));
    }

    [Test]
    public void BorrowBook_WhenBookIsAlreadyBorrowed_Fails()
    {
        var repository = new FakeBookRepository(
            new Book { Id = 1, Title = "The Hobbit", BorrowedBy = "Ada" });

        var service = new BookLoanService(repository);


        var exception = Assert.Throws<InvalidOperationException>(
            () => service.BorrowBook(1, "Grace"));


        Assert.That(exception!.Message, Is.EqualTo("The book is already on loan."));

        Assert.That(repository.Get(1)!.BorrowedBy, Is.EqualTo("Ada"));

        Assert.That(repository.UpdateCount, Is.EqualTo(0));
    }

    [Test]
    public void BorrowBook_WhenBookDoesNotExist_Fails()
    {
        var repository = new FakeBookRepository();

        var service = new BookLoanService(repository);


        var exception = Assert.Throws<InvalidOperationException>(
            () => service.BorrowBook(99, "Grace"));


        Assert.That(exception!.Message, Is.EqualTo("The book does not exist."));
    }

    [Test]
    public void BorrowBook_WithoutUserName_Fails()
    {
        var repository = new FakeBookRepository(
            new Book { Id = 1, Title = "The Hobbit" });

        var service = new BookLoanService(repository);


        Assert.Throws<ArgumentException>(
            () => service.BorrowBook(1, "   "));

        Assert.That(repository.UpdateCount, Is.EqualTo(0));
    }

    [Test]
    public void ReturnBook_WhenBorrowedByUser_ClearsBorrower()
    {
        var repository = new FakeBookRepository(
            new Book { Id = 1, Title = "The Hobbit", BorrowedBy = "Grace" });

        var service = new BookLoanService(repository);


        service.ReturnBook(1, "Grace");


        Assert.That(repository.Get(1)!.BorrowedBy, Is.Null);
    }

    [Test]
    public void ReturnBook_WhenBorrowedBySomeoneElse_Fails()
    {
        var repository = new FakeBookRepository(
            new Book { Id = 1, Title = "The Hobbit", BorrowedBy = "Ada" });

        var service = new BookLoanService(repository);


        var exception = Assert.Throws<InvalidOperationException>(
            () => service.ReturnBook(1, "Grace"));


        Assert.That(
            exception!.Message,
            Is.EqualTo("The book is on loan to somebody else."));

        Assert.That(repository.Get(1)!.BorrowedBy, Is.EqualTo("Ada"));

        Assert.That(repository.UpdateCount, Is.EqualTo(0));
    }

    [Test]
    public void ReturnBook_WhenBookIsNotBorrowed_Fails()
    {
        var repository = new FakeBookRepository(
            new Book { Id = 1, Title = "The Hobbit" });

        var service = new BookLoanService(repository);


        var exception = Assert.Throws<InvalidOperationException>(
            () => service.ReturnBook(1, "Grace"));


        Assert.That(exception!.Message, Is.EqualTo("The book is not on loan."));
    }
}
