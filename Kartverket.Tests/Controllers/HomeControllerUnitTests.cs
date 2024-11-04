using Kartverket.Controllers;
using Kartverket.Database;
using Kartverket.Models;
using Kartverket.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Kartverket.Kartverket.Tests.Controllers;

public class HomeControllerUnitTests
{

    [Fact]
    public void Index_ReturnsViewResult_WithHomeViewModel()
    {
        // Act
        var result = GetUnitUnderTest().Login();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UserLoginModel>(viewResult.Model);
        Assert.Empty(model.Username);
    }

    [Fact]
    public void Privacy_ReturnsViewResult()
    {
        // Act
        var result = GetUnitUnderTest().Privacy();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Error_ReturnsViewResult_WithErrorViewModel()
    {
        // Act
        var result = GetUnitUnderTest().Error();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ErrorViewModel>(viewResult.Model);
        Assert.NotNull(model.RequestId);
    }

    private HomeController GetUnitUnderTest()
    {
        var context = Substitute.For<ApplicationDbContext>();
        var logger = Substitute.For<ILogger<HomeController>>();
        var municipalityService = Substitute.For<MunicipalityService>();
        var userService = Substitute.For<UserService>();
        
        var controller =  new HomeController(context, logger, municipalityService, userService);
        controller.ControllerContext.HttpContext = new DefaultHttpContext();
        return controller;
    }
}