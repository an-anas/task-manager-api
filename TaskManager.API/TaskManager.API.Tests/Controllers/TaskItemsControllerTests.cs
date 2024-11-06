using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskManager.Models;
using TaskManager.API.Controllers;
using Microsoft.Extensions.Logging;
using TaskManager.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace TaskManager.API.Tests.Controllers
{
    [TestFixture]
    public class TaskItemsControllerTests
    {
        private Mock<ITaskItemService> _mockService;
        private Mock<ILogger<TaskItemsController>> _mockLogger;
        private TaskItemsController _controller;

        // Helper method to create a ClaimsPrincipal with a UserId claim
        private ClaimsPrincipal CreateClaimsPrincipal(string userId)
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));
        }

        [SetUp]
        public void Setup()
        {
            _mockService = new Mock<ITaskItemService>();
            _mockLogger = new Mock<ILogger<TaskItemsController>>();
            _controller = new TaskItemsController(_mockLogger.Object, _mockService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = CreateClaimsPrincipal("userId") }
                }
            };
        }

        [Test]
        public async Task Get_ReturnsOkResult_WithTasks()
        {
            // Arrange
            var tasks = new List<TaskItem>
            {
                new() { Id = "1", Title = "Task 1", Completed = false, UserId = "userId" },
                new() { Id = "2", Title = "Task 2", Completed = true, UserId = "userId" }
            };

            _mockService.Setup(service => service.GetAllTasksAsync("userId", It.IsAny<bool?>()))
                        .ReturnsAsync(tasks);

            // Act
            var result = await _controller.Get(completed: null) as OkObjectResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result?.StatusCode, Is.EqualTo(200));
                Assert.That(result?.Value, Is.EqualTo(tasks));
            });
        }

        [Test]
        public async Task Get_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            _mockService.Setup(service => service.GetAllTasksAsync("userId", It.IsAny<bool?>()))
                        .ThrowsAsync(new Exception("Something went wrong"));

            // Act
            var result = await _controller.Get(completed: null) as ObjectResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result?.StatusCode, Is.EqualTo(500));
                Assert.That(result?.Value, Is.EqualTo("Internal server error"));
            });
        }

        [Test]
        public async Task Get_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            _mockService.Setup(service => service.GetTaskByIdAsync("nonexistent-id", "userId"))
                        .ReturnsAsync((TaskItem?)null);

            // Act
            var result = await _controller.Get("nonexistent-id") as NotFoundResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result?.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task Get_ReturnsOk_WhenTaskExists()
        {
            // Arrange
            var taskId = "existing-task-id"; // Example task ID
            var expectedTask = new TaskItem
            {
                Id = taskId,
                Title = "Sample Task",
                Completed = false,
                UserId = "user-id"
            };

            // Mock the taskItemService to return a task when queried with the specified ID
            _mockService.Setup(service => service.GetTaskByIdAsync(taskId, "userId"))
                .ReturnsAsync(expectedTask);

            // Act
            var result = await _controller.Get(taskId) as OkObjectResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null); // Ensure the result is not null
                Assert.That(result?.StatusCode, Is.EqualTo(200)); // Ensure the response status is 200 OK
                Assert.That(result?.Value, Is.EqualTo(expectedTask)); // Ensure the returned task is the expected one
            });
        }


        [Test]
        public async Task Get_ReturnsInternalServerError_WhenExceptionOccursForGetById()
        {
            // Arrange
            _mockService.Setup(service => service.GetTaskByIdAsync("error-id", "userId"))
                        .ThrowsAsync(new Exception("Something went wrong"));

            // Act
            var result = await _controller.Get("error-id") as ObjectResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result?.StatusCode, Is.EqualTo(500));
                Assert.That(result?.Value, Is.EqualTo("Internal server error"));
            });
        }



        [Test]
        public async Task Post_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var task = new TaskItem { Id = "1", Title = "New Task", Completed = false };

            _mockService.Setup(service => service.AddTaskAsync(It.IsAny<TaskItem>()))
                        .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Post(task) as CreatedAtActionResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result?.StatusCode, Is.EqualTo(201));
                Assert.That(result?.ActionName, Is.EqualTo("Get"));
                Assert.That(((TaskItem?)result?.Value)?.Id, Is.EqualTo(task.Id));
                Assert.That(((TaskItem?)result?.Value)?.UserId, Is.EqualTo("userId")); // Assert UserId is set
            });
        }

        [Test]
        public async Task Post_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Title", "Title is required");

            var task = new TaskItem { Id = "1", Title = "New Task", Completed = false };

            // Act
            var result = await _controller.Post(task) as BadRequestObjectResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result?.StatusCode, Is.EqualTo(400));
            });
        }

        [Test]
        public async Task Post_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var task = new TaskItem { Id = "1", Title = "New Task", Completed = false };
            _mockService.Setup(service => service.AddTaskAsync(task))
                        .ThrowsAsync(new Exception("Something went wrong"));

            // Act
            var result = await _controller.Post(task) as ObjectResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result?.StatusCode, Is.EqualTo(500));
            });
        }

        [Test]
        public async Task Delete_ReturnsOk_WhenTaskIsDeleted()
        {
            // Arrange
            _mockService.Setup(service => service.DeleteTaskAsync("existing-id", "userId"))
                        .ReturnsAsync(true);

            // Act
            var result = await _controller.Delete("existing-id") as OkResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result?.StatusCode, Is.EqualTo(200));
            });
        }

        [Test]
        public async Task Delete_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            _mockService.Setup(service => service.DeleteTaskAsync("nonexistent-id", "userId"))
                        .ReturnsAsync(false);

            // Act
            var result = await _controller.Delete("nonexistent-id") as NotFoundResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result?.StatusCode, Is.EqualTo(404));
            });
        }

        [Test]
        public async Task Delete_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            _mockService.Setup(service => service.DeleteTaskAsync("error-id", "userId"))
                        .ThrowsAsync(new Exception("Something went wrong"));

            // Act
            var result = await _controller.Delete("error-id") as ObjectResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result?.StatusCode, Is.EqualTo(500));
            });
        }

        [Test]
        public async Task Patch_ReturnsBadRequest_WhenCompletedIsNull()
        {
            // Arrange
            var updateRequest = new UpdateTaskRequest { Completed = null };

            // Act
            var result = await _controller.Patch("existing-id", updateRequest) as BadRequestObjectResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result?.StatusCode, Is.EqualTo(400));
                Assert.That(result?.Value, Is.EqualTo("The 'completed' field is required."));
            });
        }

        [Test]
        public async Task Patch_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            var updateRequest = new UpdateTaskRequest { Completed = true };
            _mockService.Setup(service => service.GetTaskByIdAsync("nonexistent-id", "userId"))
                        .ReturnsAsync((TaskItem?)null);

            // Act
            var result = await _controller.Patch("nonexistent-id", updateRequest) as NotFoundResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result?.StatusCode, Is.EqualTo(404));
            });
        }

        [Test]
        public async Task Patch_ReturnsOk_WhenTaskIsUpdated()
        {
            // Arrange
            var existingTask = new TaskItem { Id = "existing-id", Title = "Existing Task", Completed = false, UserId = "userId" };
            var updateRequest = new UpdateTaskRequest { Completed = true };

            _mockService.Setup(service => service.GetTaskByIdAsync(existingTask.Id, existingTask.UserId))
                        .ReturnsAsync(existingTask);

            _mockService.Setup(service => service.UpdateTaskAsync(existingTask.Id, existingTask.UserId, existingTask))
                        .ReturnsAsync(new UpdateResult { Found = true, Updated = true });

            // Act
            var result = await _controller.Patch(existingTask.Id, updateRequest) as OkResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result?.StatusCode, Is.EqualTo(200));
            });
        }

        [Test]
        public async Task Patch_ReturnsNotFound_WhenUpdateResultFoundIsFalse()
        {
            // Arrange
            var existingTask = new TaskItem { Id = "existing-id", Title = "Existing Task", Completed = false, UserId = "userId" };
            var updateRequest = new UpdateTaskRequest { Completed = true };

            _mockService.Setup(service => service.UpdateTaskAsync(existingTask.Id, existingTask.UserId, existingTask))
                        .ReturnsAsync(new UpdateResult { Found = false, Updated = false });

            // Act
            var result = await _controller.Patch(existingTask.Id, updateRequest) as NotFoundResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result?.StatusCode, Is.EqualTo(404));
            });
        }

        [Test]
        public async Task Patch_ReturnsNoContent_WhenUpdateResultUpdatedIsFalse()
        {
            // Arrange
            var existingTask = new TaskItem { Id = "existing-id", Title = "Existing Task", Completed = false, UserId = "userId" };
            var updateRequest = new UpdateTaskRequest { Completed = true };

            _mockService.Setup(service => service.GetTaskByIdAsync(existingTask.Id, existingTask.UserId))
                        .ReturnsAsync(existingTask);

            _mockService.Setup(service => service.UpdateTaskAsync(existingTask.Id, existingTask.UserId, existingTask))
                        .ReturnsAsync(new UpdateResult { Found = true, Updated = false });

            // Act
            var result = await _controller.Patch(existingTask.Id, updateRequest) as NoContentResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result?.StatusCode, Is.EqualTo(204));
            });
        }

        [Test]
        public async Task Patch_ReturnsInternalServerError_WhenExceptionIsThrown()
        {
            // Arrange
            var existingTask = new TaskItem { Id = "existing-id", Title = "Existing Task", Completed = false, UserId = "userId" };
            var updateRequest = new UpdateTaskRequest { Completed = true };

            _mockService.Setup(service => service.GetTaskByIdAsync(existingTask.Id, existingTask.UserId))
                        .ReturnsAsync(existingTask);

            _mockService.Setup(service => service.UpdateTaskAsync(existingTask.Id, existingTask.UserId, existingTask))
                        .ThrowsAsync(new Exception("Something went wrong"));

            // Act
            var result = await _controller.Patch(existingTask.Id, updateRequest) as ObjectResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result?.StatusCode, Is.EqualTo(500));
            });
        }

        [Test]
        public async Task Put_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            var updatedTask = new TaskItem { Id = "nonexistent-id", Title = "Updated Task", Completed = true, UserId = "userId" };

            _mockService.Setup(service => service.UpdateTaskAsync(updatedTask.Id, updatedTask.UserId, updatedTask))
                .ReturnsAsync(new UpdateResult { Found = false, Updated = false });

            // Act
            var result = await _controller.Put(updatedTask.Id, updatedTask) as NotFoundResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result?.StatusCode, Is.EqualTo(404));
            });
        }

        [Test]
        public async Task Put_ReturnsOk_WhenTaskIsUpdated()
        {
            // Arrange
            var updatedTask = new TaskItem { Id = "existing-id", Title = "Updated Task", Completed = true, UserId = "userId" };

            _mockService.Setup(service => service.UpdateTaskAsync(updatedTask.Id, updatedTask.UserId, updatedTask))
                .ReturnsAsync(new UpdateResult { Found = true, Updated = true });

            // Act
            var result = await _controller.Put(updatedTask.Id, updatedTask) as OkResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result?.StatusCode, Is.EqualTo(200));
            });
        }

        [Test]
        public async Task Put_ReturnsNoContent_WhenTaskIsNotUpdated()
        {
            // Arrange
            var updatedTask = new TaskItem { Id = "existing-id", Title = "Updated Task", Completed = true, UserId = "userId" };

            _mockService.Setup(service => service.UpdateTaskAsync(updatedTask.Id, updatedTask.UserId, updatedTask))
                .ReturnsAsync(new UpdateResult { Found = true, Updated = false });

            // Act
            var result = await _controller.Put(updatedTask.Id, updatedTask) as NoContentResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result?.StatusCode, Is.EqualTo(204));
            });
        }

        [Test]
        public async Task Put_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var updatedTask = new TaskItem { Id = "existing-id", Title = "Updated Task", Completed = true, UserId = "userId" };

            _mockService.Setup(service => service.UpdateTaskAsync(updatedTask.Id, updatedTask.UserId, updatedTask))
                .ThrowsAsync(new Exception("Something went wrong"));

            // Act
            var result = await _controller.Put(updatedTask.Id, updatedTask) as ObjectResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result?.StatusCode, Is.EqualTo(500));
            });
        }

        [Test]
        public async Task Put_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Title", "Title is required");

            var updatedTask = new TaskItem { Id = "existing-id", Title = "Updated Task", Completed = true, UserId = "userId" };

            // Act
            var result = await _controller.Put(updatedTask.Id, updatedTask) as BadRequestObjectResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result?.StatusCode, Is.EqualTo(400));
            });
        }
    }
}
