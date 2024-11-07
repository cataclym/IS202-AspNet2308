using Kartverket.Controllers;
using Kartverket.Database;
using Kartverket.Models;
using Kartverket.Services;  
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Kartverket.Tests.Controllers;

public class HomeControllerUnitTests
{

    [Fact]
    public void Index_ReturnsViewResult_WithHomeViewModel()
    {
        // Act
        var result = GetUnitUnderTest().Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.Model);
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
        // Substitutt for logger
        var mockLogger = Substitute.For<ILogger<HomeController>>();
        var mockLoggerMs = Substitute.For<ILogger<MunicipalityService>>();
        var mockLoggerUs = Substitute.For<ILogger<UserService>>();
        
        // Substitutt for database kontext
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMemoryCache(null)
            .Options;
        var mockContext = Substitute.For<ApplicationDbContext>(options);
        
        // Substitutt for KommuneService
        var mockHttpClient = Substitute.For<HttpClient>();
        var mockMunicipalityService = Substitute.For<MunicipalityService>(mockHttpClient, mockLoggerMs);
        
        // Substitutt for BrukerService
        var mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var mockUserService = Substitute.For<UserService>(mockContext, mockLoggerUs, mockHttpContextAccessor);
        
        // Returner homecontroller med substituerte parametere i konstruktøren
        var homeController =  new HomeController(mockContext, mockLogger, mockMunicipalityService, mockUserService);
        homeController.ControllerContext.HttpContext = new DefaultHttpContext();
        return homeController;
    }
}