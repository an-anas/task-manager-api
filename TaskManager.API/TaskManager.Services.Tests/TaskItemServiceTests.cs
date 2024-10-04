using Moq;
using TaskManager.DataAccess.Repository;
using TaskManager.Models;
using TaskManager.Services.Interfaces;

namespace TaskManager.Services.Tests
{
    [TestFixture]
    public class TaskItemServiceTests
    {
        private Mock<ITaskItemRepository> _repositoryMock;
        private ITaskItemService _taskItemService;

        [SetUp]
        public void Setup()
        {
            _repositoryMock = new Mock<ITaskItemRepository>();
            _taskItemService = new TaskItemService(_repositoryMock.Object);
        }

        [Test]
        public async Task GetAllTasksAsync_ReturnsTasks()
        {
            // Arrange
            var tasks = new List<TaskItem>
            {
                new TaskItem { Id = "1", Title = "Task 1", Completed = false },
                new TaskItem { Id = "2", Title = "Task 2", Completed = true }
            };
            _repositoryMock.Setup(repo => repo.GetAllTasksAsync(null)).ReturnsAsync(tasks);

            // Act
            var result = await _taskItemService.GetAllTasksAsync(null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("Task 1", result.First().Title);
        }

        [Test]
        public async Task GetTaskByIdAsync_ValidId_ReturnsTask()
        {
            // Arrange
            var task = new TaskItem { Id = "1", Title = "Task 1", Completed = false };
            _repositoryMock.Setup(repo => repo.GetTaskByIdAsync("1")).ReturnsAsync(task);

            // Act
            var result = await _taskItemService.GetTaskByIdAsync("1");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Task 1", result?.Title);
        }

        [Test]
        public async Task AddTaskAsync_CallsRepositoryMethod()
        {
            // Arrange
            var task = new TaskItem { Id = "1", Title = "Task 1", Completed = false };

            // Act
            await _taskItemService.AddTaskAsync(task);

            // Assert
            _repositoryMock.Verify(repo => repo.AddTaskAsync(task), Times.Once);
        }

        [Test]
        public async Task UpdateTaskAsync_ValidId_CallsRepositoryMethod()
        {
            // Arrange
            var updatedTask = new TaskItem { Id = "1", Title = "Updated Task", Completed = false };

            // Act
            await _taskItemService.UpdateTaskAsync("1", updatedTask);

            // Assert
            _repositoryMock.Verify(repo => repo.UpdateTaskAsync("1", updatedTask), Times.Once);
        }

        [Test]
        public async Task DeleteTaskAsync_ValidId_CallsRepositoryMethod()
        {
            // Arrange
            string taskId = "1";

            // Act
            await _taskItemService.DeleteTaskAsync(taskId);

            // Assert
            _repositoryMock.Verify(repo => repo.DeleteTaskAsync(taskId), Times.Once);
        }

        [Test]
        public async Task GetAllTasksAsync_WithCompletedFilter_ReturnsFilteredTasks()
        {
            // Arrange
            var tasks = new List<TaskItem>
            {
                new TaskItem { Id = "1", Title = "Task 1", Completed = false },
                new TaskItem { Id = "2", Title = "Task 2", Completed = true }
            };
            _repositoryMock.Setup(repo => repo.GetAllTasksAsync(true)).ReturnsAsync(new List<TaskItem> { tasks[1] });

            // Act
            var result = await _taskItemService.GetAllTasksAsync(true);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("Task 2", result.First().Title);
        }
    }
}
