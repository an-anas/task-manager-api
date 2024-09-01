using MongoDB.Bson.Serialization;
using Moq;
using MongoDB.Driver;
using TaskManager.DataAccess.Context;
using TaskManager.DataAccess.Repository;
using TaskManager.Models;

namespace TaskManager.DataAccess.Tests.Repository
{
    [TestFixture]
    public class TaskItemRepositoryTests
    {
        private Mock<IMongoDbContext> _mockContext;
        private Mock<IMongoCollection<TaskItem>> _mockCollection;
        private ITaskItemRepository _repository;

        [SetUp]
        public void SetUp()
        {
            _mockCollection = new Mock<IMongoCollection<TaskItem>>();

            var mockContext = new Mock<IMongoDbContext>();
            mockContext.Setup(c => c.TaskItems).Returns(_mockCollection.Object);

            _repository = new TaskItemRepository(mockContext.Object);
        }

        [Test]
        public async Task GetAllTasksAsync_ReturnsAllTasks()
        {
            // Arrange
            var tasks = new List<TaskItem>
            {
                new TaskItem { Id = "1", Title = "Task 1", Completed = false },
                new TaskItem { Id = "2", Title = "Task 2", Completed = true }
            };

            var mockCursor = new Mock<IAsyncCursor<TaskItem>>();
            mockCursor.Setup(c => c.Current).Returns(tasks);
            mockCursor.SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(true)
                .Returns(false);

            // Set up the Find method to return the mockCursor
            _mockCollection.Setup(c => c.Find(It.IsAny<FilterDefinition<TaskItem>>() as FilterDefinition<TaskItem>, 
                    It.IsAny<FindOptions<TaskItem, TaskItem>>() as FindOptions<TaskItem, TaskItem>))
                .Returns(mockCursor.Object);

            // Act
            var result = await _repository.GetAllTasksAsync(null);

            // Assert
            Assert.That(result, Is.EqualTo(tasks));
            _mockCollection.Verify(c => c.Find(It.IsAny<FilterDefinition<TaskItem>>(), It.IsAny<FindOptions<TaskItem, TaskItem>>()), Times.Once);
        }
    }


    [Test]
        public async Task AddTaskAsync_AddsNewTask()
        {
            // Arrange
            var taskItem = new TaskItem { Id = "new-id", Title = "New Task", Completed = false };

            // Act
            await _repository.AddTaskAsync(taskItem);

            // Assert
            _mockCollection.Verify(c => c.InsertOneAsync(taskItem, null, default), Times.Once);
        }

        [Test]
        public async Task UpdateTaskAsync_UpdatesTask_WhenTaskExists()
        {
            // Arrange
            var taskItem = new TaskItem { Id = "existing-id", Title = "Updated Task", Completed = true };

            var updateResult = new Mock<ReplaceOneResult>();
            updateResult.Setup(r => r.ModifiedCount).Returns(1);

            _mockCollection.Setup(c => c.ReplaceOneAsync(
                    It.IsAny<FilterDefinition<TaskItem>>(),
                    taskItem,
                    It.IsAny<ReplaceOptions>(), // Specify this parameter to resolve ambiguity
                    default(CancellationToken)))
                .ReturnsAsync(updateResult.Object);

            // Act
            var result = await _repository.UpdateTaskAsync("existing-id", taskItem);

            // Assert
            Assert.That(result, Is.True);
        }


        [Test]
        public async Task DeleteTaskAsync_DeletesTask_WhenTaskExists()
        {
            // Arrange
            var deleteResult = new Mock<DeleteResult>();
            deleteResult.Setup(r => r.DeletedCount).Returns(1);

            _mockCollection.Setup(c => c.DeleteOneAsync(It.IsAny<FilterDefinition<TaskItem>>(), default))
                .ReturnsAsync(deleteResult.Object);

            // Act
            var result = await _repository.DeleteTaskAsync("existing-id");

            // Assert
            Assert.That(result, Is.True);
        }
    }
}
