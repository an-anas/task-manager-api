using Moq;
using TaskManager.DataAccess.Repository;
using TaskManager.Models;

namespace TaskManager.DataAccess.Tests.Repository
{
    [TestFixture]
    public class TaskItemRepositoryTests
    {
        [Test]
        public async Task GetAllTasksAsync_ShouldReturnOnlyTasksForSpecifiedUser_WhenCompletedIsNull()
        {
            // Arrange
            var mockRepository = new Mock<ITaskItemRepository>();
            var userId = "testUserId";

            var testTasks = new List<TaskItem>
            {
                new() { Id = "1", Title = "User 1 Task", Completed = false, UserId = userId },
                new() { Id = "2", Title = "User 2 Task", Completed = true, UserId = "otherUserId" }
            };

            mockRepository.Setup(repo => repo.GetAllTasksAsync(userId, null))
                          .ReturnsAsync(testTasks.Where(task => task.UserId == userId));

            var repository = mockRepository.Object;

            // Act
            var result = await repository.GetAllTasksAsync(userId, null);
            var taskItems = result.ToList();

            // Assert
            Assert.That(taskItems, Has.Count.EqualTo(1));
            Assert.That(taskItems, Has.Exactly(1).Matches<TaskItem>(t => t.UserId == userId));
            Assert.That(taskItems, Has.Exactly(0).Matches<TaskItem>(t => t.UserId == "otherUserId"));
        }

        [Test]
        public async Task GetTaskByIdAsync_ShouldReturnTaskForSpecifiedUser_WhenTaskExists()
        {
            // Arrange
            var mockRepository = new Mock<ITaskItemRepository>();
            var userId = "testUserId";

            var task = new TaskItem { Id = "1", Title = "Test Task", Completed = false, UserId = userId };

            mockRepository.Setup(repo => repo.GetTaskByIdAsync("1", userId))
                          .ReturnsAsync(task);

            var repository = mockRepository.Object;

            // Act
            var result = await repository.GetTaskByIdAsync("1", userId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo("1"));
            Assert.That(result.UserId, Is.EqualTo(userId));
        }

        [Test]
        public async Task GetTaskByIdAsync_ShouldReturnNull_WhenTaskDoesNotBelongToUser()
        {
            // Arrange
            var mockRepository = new Mock<ITaskItemRepository>();
            var userId = "testUserId";

            mockRepository.Setup(repo => repo.GetTaskByIdAsync("1", userId))
                          .ReturnsAsync((TaskItem?)null);

            var repository = mockRepository.Object;

            // Act
            var result = await repository.GetTaskByIdAsync("1", userId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task AddTaskAsync_ShouldAddTask()
        {
            // Arrange
            var mockRepository = new Mock<ITaskItemRepository>();

            var task = new TaskItem { Id = "1", Title = "New Task", Completed = false, UserId = "testUserId" };

            mockRepository.Setup(repo => repo.AddTaskAsync(task))
                          .Returns(Task.CompletedTask);

            var repository = mockRepository.Object;

            // Act
            await repository.AddTaskAsync(task);

            // Assert
            mockRepository.Verify(repo => repo.AddTaskAsync(task), Times.Once);
        }

        [Test]
        public async Task UpdateTaskAsync_ShouldReturnUpdatedTrue_WhenTaskIsUpdatedForUser()
        {
            // Arrange
            var mockRepository = new Mock<ITaskItemRepository>();
            var userId = "testUserId";
            var updateResult = new UpdateResult { Found = true, Updated = true };

            var updatedTask = new TaskItem { Id = "1", Title = "Updated Task", Completed = true, UserId = userId };

            mockRepository.Setup(repo => repo.UpdateTaskAsync("1", userId, updatedTask))
                          .ReturnsAsync(updateResult);

            var repository = mockRepository.Object;

            // Act
            var result = await repository.UpdateTaskAsync("1", userId, updatedTask);

            // Assert
            Assert.That(result, Is.EqualTo(updateResult));
        }

        [Test]
        public async Task UpdateTaskAsync_ShouldReturnUpdatedFalse_WhenTaskBelongsToOtherUser()
        {
            // Arrange
            var mockRepository = new Mock<ITaskItemRepository>();
            var userId = "testUserId";
            var updateResult = new UpdateResult { Found = false, Updated = false };

            var updatedTask = new TaskItem { Id = "1", Title = "Updated Task", Completed = true, UserId = "otherUserId" };

            // Simulate trying to update another user's task
            mockRepository.Setup(repo => repo.UpdateTaskAsync("1", userId, updatedTask))
                          .ReturnsAsync(updateResult);

            var repository = mockRepository.Object;

            // Act
            var result = await repository.UpdateTaskAsync("1", userId, updatedTask);

            // Assert
            Assert.That(result, Is.EqualTo(updateResult));
            Assert.That(result.Updated, Is.False); // Should not allow update
        }

        [Test]
        public async Task DeleteTaskAsync_ShouldReturnTrue_WhenTaskIsDeletedForUser()
        {
            // Arrange
            var mockRepository = new Mock<ITaskItemRepository>();
            var userId = "testUserId";

            mockRepository.Setup(repo => repo.DeleteTaskAsync("1", userId))
                          .ReturnsAsync(true);

            var repository = mockRepository.Object;

            // Act
            var result = await repository.DeleteTaskAsync("1", userId);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task DeleteTaskAsync_ShouldReturnFalse_WhenTaskBelongsToOtherUser()
        {
            // Arrange
            var mockRepository = new Mock<ITaskItemRepository>();
            var userId = "testUserId";

            // Simulate trying to delete another user's task
            mockRepository.Setup(repo => repo.DeleteTaskAsync("1", userId))
                          .ReturnsAsync(false);

            var repository = mockRepository.Object;

            // Act
            var result = await repository.DeleteTaskAsync("1", userId);

            // Assert
            Assert.That(result, Is.False); // Should not allow deletion
        }

        [Test]
        public async Task DeleteTaskAsync_ShouldReturnFalse_WhenTaskDoesNotExistForUser()
        {
            // Arrange
            var mockRepository = new Mock<ITaskItemRepository>();
            var userId = "testUserId";

            mockRepository.Setup(repo => repo.DeleteTaskAsync("nonexistent-id", userId))
                          .ReturnsAsync(false);

            var repository = mockRepository.Object;

            // Act
            var result = await repository.DeleteTaskAsync("nonexistent-id", userId);

            // Assert
            Assert.That(result, Is.False);
        }
    }
}
