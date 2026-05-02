using ToDoApp.Controllers;

namespace Tests
{
    public class ErrorViewModelTests
    {
        // Tests ErrorViewModel logic
        [Fact]
        public void ShowRequestId_ReturnsTrue_IfIdIsNotNull()
        {
            var model = new ErrorViewModel { RequestId = "123" };
            Assert.True(model.ShowRequestId);

            var emptyModel = new ErrorViewModel { RequestId = null };
            Assert.False(emptyModel.ShowRequestId);
        }
    }
}
