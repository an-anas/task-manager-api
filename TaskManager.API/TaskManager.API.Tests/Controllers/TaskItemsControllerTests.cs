using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManager.API.Controllers;


namespace TaskManager.API.Tests.Controllers
{
    [TestFixture]
    public class TaskItemsControllerTests
    {
        [Test]
        public void Get_ReturnsOkResult()
        {
            // Arrange
            var logger = new Mock<ILogger<TaskItemsController>>();
            var controller = new TaskItemsController(logger.Object);

            // Act
            var result = controller.Get();

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            Assert.That(result, Is.Not.Null);
        }
    }
}
