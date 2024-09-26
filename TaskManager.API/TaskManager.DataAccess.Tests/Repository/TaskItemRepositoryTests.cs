using Moq;
using TaskManager.DataAccess.Repository;
using TaskManager.Models;

namespace TaskManager.DataAccess.Tests.Repository
{
    [TestFixture]
    public class TaskItemRepositoryTests
    {
        [Test]
        public async Task GetAllTasksAsync_ShouldReturnAllTasks_WhenCompletedIsNull()
        {
            // Arrange
            var mockRepository = new Mock<ITaskItemRepository>();

            var testTasks = new List<TaskItem>
            {
                new() { Id = "1", Title = "Test Task 1", Completed = false },
                new() { Id = "2", Title = "Test Task 2", Completed = true }
            };

            mockRepository.Setup(repo => repo.GetAllTasksAsync(null))
                          .ReturnsAsync(testTasks);

            var repository = mockRepository.Object;

            // Act
            var result = await repository.GetAllTasksAsync(null);
            var taskItems = result.ToList();

            // Assert
            Assert.That(taskItems, Has.Count.EqualTo(2));
            Assert.That(taskItems, Has.Exactly(1).Matches<TaskItem>(t => t.Title == "Test Task 1"));
            Assert.That(taskItems, Has.Exactly(1).Matches<TaskItem>(t => t.Title == "Test Task 2"));
        }

        [Test]
        public async Task GetAllTasksAsync_ShouldReturnCompletedTasks_WhenCompletedIsTrue()
        {
            // Arrange
            var mockRepository = new Mock<ITaskItemRepository>();

            var completedTasks = new List<TaskItem>
            {
                new() { Id = "2", Title = "Test Task 2", Completed = true }
            };

            mockRepository.Setup(repo => repo.GetAllTasksAsync(true))
                          .ReturnsAsync(completedTasks);

            var repository = mockRepository.Object;

            // Act
            var result = await repository.GetAllTasksAsync(true);
            var taskItems = result.ToList();

            // Assert
            Assert.That(taskItems, Has.Count.EqualTo(1));
            Assert.That(taskItems, Has.Exactly(1).Matches<TaskItem>(t => t.Completed));
        }

        [Test]
        public async Task GetAllTasksAsync_ShouldReturnIncompleteTasks_WhenCompletedIsFalse()
        {
            // Arrange
            var mockRepository = new Mock<ITaskItemRepository>();

            var incompleteTasks = new List<TaskItem>
            {
                new() { Id = "1", Title = "Test Task 1", Completed = false }
            };

            mockRepository.Setup(repo => repo.GetAllTasksAsync(false))
                          .ReturnsAsync(incompleteTasks);

            var repository = mockRepository.Object;

            // Act
            var result = await repository.GetAllTasksAsync(false);
            var taskItems = result.ToList();

            // Assert
            Assert.That(taskItems, Has.Count.EqualTo(1));
            Assert.That(taskItems, Has.Exactly(1).Matches<TaskItem>(t => !t.Completed));
        }

        [Test]
        public async Task GetTaskByIdAsync_ShouldReturnTask_WhenTaskExists()
        {
            // Arrange
            var mockRepository = new Mock<ITaskItemRepository>();

            var task = new TaskItem { Id = "1", Title = "Test Task", Completed = false };

            mockRepository.Setup(repo => repo.GetTaskByIdAsync("1"))
                          .ReturnsAsync(task);

            var repository = mockRepository.Object;

            // Act
            var result = await repository.GetTaskByIdAsync("1");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo("1"));
            Assert.That(result.Title, Is.EqualTo("Test Task"));
            Assert.That(result.Completed, Is.False);
        }

        [Test]
        public async Task GetTaskByIdAsync_ShouldReturnNull_WhenTaskDoesNotExist()
        {
            // Arrange
            var mockRepository = new Mock<ITaskItemRepository>();

            mockRepository.Setup(repo => repo.GetTaskByIdAsync("nonexistent-id"))
                          .ReturnsAsync((TaskItem?)null);

            var repository = mockRepository.Object;

            // Act
            var result = await repository.GetTaskByIdAsync("nonexistent-id");

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task AddTaskAsync_ShouldAddTask()
        {
            // Arrange
            var mockRepository = new Mock<ITaskItemRepository>();

            var task = new TaskItem { Id = "1", Title = "New Task", Completed = false };

            mockRepository.Setup(repo => repo.AddTaskAsync(task))
                          .Returns(Task.CompletedTask);

            var repository = mockRepository.Object;

            // Act
            await repository.AddTaskAsync(task);

            // Assert
            mockRepository.Verify(repo => repo.AddTaskAsync(task), Times.Once);
        }

        [Test]
        public async Task UpdateTaskAsync_ShouldReturnUpdatedTrue_WhenTaskIsUpdated()
        {
            // Arrange
            var mockRepository = new Mock<ITaskItemRepository>();
            var updateResult = new UpdateResult { Found = true, Updated = true };

            var updatedTask = new TaskItem { Id = "1", Title = "Updated Task", Completed = true };

            mockRepository.Setup(repo => repo.UpdateTaskAsync("1", updatedTask))
                          .ReturnsAsync(updateResult);

            var repository = mockRepository.Object;

            // Act
            var result = await repository.UpdateTaskAsync("1", updatedTask);

            // Assert
            Assert.That(result, Is.EqualTo(updateResult));
        }

        [Test]
        public async Task UpdateTaskAsync_ShouldReturnUpdatedFalse_WhenTaskIsNotUpdated()
        {
            // Arrange
            var mockRepository = new Mock<ITaskItemRepository>();

            var updatedTask = new TaskItem { Id = "1", Title = "Updated Task", Completed = true };
            var updateResult = new UpdateResult { Found = true, Updated = false };

            mockRepository.Setup(repo => repo.UpdateTaskAsync("1", updatedTask))
                          .ReturnsAsync(updateResult);

            var repository = mockRepository.Object;

            // Act
            var result = await repository.UpdateTaskAsync("1", updatedTask);

            // Assert
            Assert.That(result, Is.EqualTo(updateResult));
        }

        [Test]
        public async Task DeleteTaskAsync_ShouldReturnTrue_WhenTaskIsDeleted()
        {
            // Arrange
            var mockRepository = new Mock<ITaskItemRepository>();

            mockRepository.Setup(repo => repo.DeleteTaskAsync("1"))
                          .ReturnsAsync(true);

            var repository = mockRepository.Object;

            // Act
            var result = await repository.DeleteTaskAsync("1");

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task DeleteTaskAsync_ShouldReturnFalse_WhenTaskIsNotDeleted()
        {
            // Arrange
            var mockRepository = new Mock<ITaskItemRepository>();

            mockRepository.Setup(repo => repo.DeleteTaskAsync("1"))
                          .ReturnsAsync(false);

            var repository = mockRepository.Object;

            // Act
            var result = await repository.DeleteTaskAsync("1");

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task DeleteTaskAsync_ShouldReturnFalse_WhenTaskDoesNotExist()
        {
            // Arrange
            var mockRepository = new Mock<ITaskItemRepository>();

            mockRepository.Setup(repo => repo.DeleteTaskAsync("nonexistent-id"))
                          .ReturnsAsync(false);

            var repository = mockRepository.Object;

            // Act
            var result = await repository.DeleteTaskAsync("nonexistent-id");

            // Assert
            Assert.That(result, Is.False);
        }
    }
}
