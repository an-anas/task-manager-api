using Moq;
using TaskManager.DataAccess.Repository;
using TaskManager.Models;

namespace TaskManager.Services.Tests
{
    [TestFixture]
    public class TaskItemServiceTests
    {
        private Mock<ITaskItemRepository> _repositoryMock;
        private TaskItemService _taskItemService;

        [SetUp]
        public void Setup()
        {
            _repositoryMock = new Mock<ITaskItemRepository>();
            _taskItemService = new TaskItemService(_repositoryMock.Object);
        }

        [Test]
        public void GetAllTasksAsync_CallsRepositoryMethod()
        {
            // Act
            var result = _taskItemService.GetAllTasksAsync("userId", null);

            // Assert
            _repositoryMock.Verify(repo => repo.GetAllTasksAsync("userId", null), Times.Once);
        }

        [Test]
        public void GetTaskByIdAsync_CallsRepositoryMethod()
        {
            // Act
            var result = _taskItemService.GetTaskByIdAsync("taskId", "userId");

            // Assert
            _repositoryMock.Verify(repo => repo.GetTaskByIdAsync("taskId", "userId"), Times.Once);
        }

        [Test]
        public void AddTaskAsync_CallsRepositoryMethod()
        {
            // Arrange
            var task = new TaskItem { Id = "1", Title = "Task", UserId = "userId", Completed = false };

            // Act
            var result = _taskItemService.AddTaskAsync(task);

            // Assert
            _repositoryMock.Verify(repo => repo.AddTaskAsync(task), Times.Once);
        }

        [Test]
        public void UpdateTaskAsync_CallsRepositoryMethod()
        {
            // Arrange
            var updatedTask = new TaskItem { Id = "1", Title = "Updated Task", UserId = "userId", Completed = false };

            // Act
            var result = _taskItemService.UpdateTaskAsync("taskId", "userId", updatedTask);

            // Assert
            _repositoryMock.Verify(repo => repo.UpdateTaskAsync("taskId", "userId", updatedTask), Times.Once);
        }

        [Test]
        public void DeleteTaskAsync_CallsRepositoryMethod()
        {
            // Act
            var result = _taskItemService.DeleteTaskAsync("taskId", "userId");

            // Assert
            _repositoryMock.Verify(repo => repo.DeleteTaskAsync("taskId", "userId"), Times.Once);
        }
    }
}
