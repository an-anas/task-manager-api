using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using TaskManager.Models;
using TaskManager.Services;
using TaskManager.API.Controllers;
using Microsoft.Extensions.Logging;

namespace TaskManager.API.Tests.Controllers
{
    [TestFixture]
    public class TaskItemsControllerTests
    {
        private Mock<ITaskItemService> _mockService;
        private Mock<ILogger<TaskItemsController>> _mockLogger;
        private TaskItemsController _controller;

        [SetUp]
        public void Setup()
        {
            _mockService = new Mock<ITaskItemService>();
            _mockLogger = new Mock<ILogger<TaskItemsController>>();
            _controller = new TaskItemsController(_mockLogger.Object, _mockService.Object);
        }

        [Test]
        public async Task Get_ReturnsOkResult_WithTasks()
        {
            // Arrange
            var tasks = new List<TaskItem>
            {
                new() { Id = "1", Title = "Task 1", Completed = false },
                new() { Id = "2", Title = "Task 2", Completed = true }
            };

            _mockService.Setup(service => service.GetAllTasksAsync(It.IsAny<bool?>()))
                        .ReturnsAsync(tasks);

            // Act
            var result = await _controller.Get(completed: null) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(200));
            Assert.That(result.Value, Is.EqualTo(tasks));
        }

        [Test]
        public async Task Get_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            _mockService.Setup(service => service.GetTaskByIdAsync(It.IsAny<string>()))
                        .ReturnsAsync((TaskItem)null);

            // Act
            var result = await _controller.Get("nonexistent-id") as NotFoundResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task Post_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var task = new TaskItem { Id = "1", Title = "New Task", Completed = false };

            _mockService.Setup(service => service.AddTaskAsync(task))
                        .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Post(task) as CreatedAtActionResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(201));
            Assert.That(result.ActionName, Is.EqualTo("Get"));
            Assert.That(((TaskItem)result.Value).Id, Is.EqualTo(task.Id));
        }

        [Test]
        public async Task Delete_ReturnsOk_WhenTaskIsDeleted()
        {
            // Arrange
            _mockService.Setup(service => service.DeleteTaskAsync(It.IsAny<string>()))
                        .ReturnsAsync(true);

            // Act
            var result = await _controller.Delete("existing-id") as OkResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task Patch_ReturnsBadRequest_WhenCompletedIsNull()
        {
            // Act
            var result = await _controller.Patch("existing-id", completed: null) as BadRequestObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(400));
            Assert.That(result.Value, Is.EqualTo("The 'completed' query parameter is required."));
        }

        [Test]
        public async Task Patch_ReturnsOk_WhenTaskIsUpdated()
        {
            // Arrange
            var existingTask = new TaskItem { Id = "existing-id", Title = "Existing Task", Completed = false };

            _mockService.Setup(service => service.GetTaskByIdAsync(It.IsAny<string>()))
                        .ReturnsAsync(existingTask);
            _mockService.Setup(service => service.UpdateTaskAsync(It.IsAny<string>(), It.IsAny<TaskItem>()))
                        .ReturnsAsync(true);

            // Act
            var result = await _controller.Patch("existing-id", completed: true) as OkResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(200));
        }

[Test]
public async Task Put_ReturnsBadRequest_WhenModelStateIsInvalid()
{
    // Arrange
    _controller.ModelState.AddModelError("Title", "Title is required");

    var task = new TaskItem { Id = "existing-id", Title = "Updated Task", Completed = true };

    // Act
    var result = await _controller.Put("existing-id", task);

    // Assert
    Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
}


        [Test]
        public async Task Put_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            var task = new TaskItem { Id = "existing-id", Title = "Updated Task", Completed = true };

            _mockService.Setup(service => service.UpdateTaskAsync(It.IsAny<string>(), It.IsAny<TaskItem>()))
                        .ReturnsAsync(false);

            // Act
            var result = await _controller.Put("existing-id", task) as NotFoundResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }
    }
}
