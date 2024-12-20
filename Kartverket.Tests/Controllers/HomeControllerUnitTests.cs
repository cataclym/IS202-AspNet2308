using Kartverket.Controllers;
using Kartverket.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kartverket.Tests.Controllers;

public class HomeControllerUnitTests
{

    [Fact]
    public void Index_ReturnsViewResult_WithHomeViewModel()
    {
        // Act
        var result = GetUnitUnderTest().Index();

        // Assert
        Assert.IsType<ViewResult>(result);
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
        // Returner homecontroller med substituerte parametere i konstruktøren
        var homeController = new HomeController(null, null);
        homeController.ControllerContext.HttpContext = new DefaultHttpContext();
        return homeController;
    }
}