using Kartverket.Controllers;
using Kartverket.Database;
using Kartverket.Models;
using Kartverket.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Kartverket.Tests.Controllers;

public class AccountControllerUnitTests
{
    private ILogger<AccountController> _logger;
    private ApplicationDbContext _applicationDbContext;
    private IUserService _userService;

    private AccountController GetUnitUnderTest()
    {
        // Returner accountcontroller med substituerte parametere i konstruktøren
        _logger = Substitute.For<ILogger<AccountController>>();
        _userService = Substitute.For<IUserService>();
        var controller = new AccountController(null, _logger, _userService);
        controller.ControllerContext.HttpContext = new DefaultHttpContext();
        return controller;
    }

    [Fact]
    public void UsingUserRegistrationReturnsNullViewDataReturnUrl()
    {
        AccountController unit = GetUnitUnderTest();
        var result = unit.UserRegistration() as ViewResult;
        var viewData = result?.ViewData;
        Assert.Null(viewData?["ReturnUrl"]);
    }
}