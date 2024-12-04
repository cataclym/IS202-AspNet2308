using Kartverket.Controllers;
using Kartverket.Database;
using Kartverket.Models;
using Kartverket.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Kartverket.Tests.Controllers;

public class ReportsControllerUnitTests
{
    private ILogger<ReportsController> _logger;
    private IMunicipalityService _municipalityService;
    private IUserService _userService;

    private ReportsController GetUnitUnderTest()
    {
        // Returner homecontroller med substituerte parametere i konstruktøren
        _logger = Substitute.For<ILogger<ReportsController>>();
        _municipalityService = Substitute.For<IMunicipalityService>();
        _userService = Substitute.For<IUserService>();
        
        var homeController = new ReportsController(null, _logger, _municipalityService, null, _userService);
        homeController.ControllerContext.HttpContext = new DefaultHttpContext();
        return homeController;
    }

    [Fact]
    public void UsingMapReportReturnsNullModel()
    {
        ReportsController unit = GetUnitUnderTest();
        var result = unit.MapReport() as ViewResult;
        var model = result.Model as ReportViewModel;
        Assert.Null(model);
    }
}