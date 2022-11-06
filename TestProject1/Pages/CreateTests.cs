using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using TestProject1.Fakes;
using WebApplication1.Models;
using WebApplication1.Pages.LoanProspects;

namespace TestProject1.Pages
{
    public class CreateTests
    {
        ILogger<CreateModel> logger = new LoggerFake<CreateModel>();
        Mock<IEmailSender> emailSender = new Mock<IEmailSender>();
        FakeDbContext context = FakeDbContext.ContextFactory();

        [Fact]
        public async Task OnPostAsync_IfSave_RedirectsToIndex()
        {
            var controller = new CreateModel(context, logger, emailSender.Object);
            var loan = new LoanProspect
            {
                IsSave = true,
                Name = "Test name",
                InterestRate = .05,
                LoanAmount = 10000,
                TermMonths = 36
            };
            controller.LoanProspect = loan;
            var result = await controller.OnPostAsync();
            var redirectResult = result as RedirectToPageResult;
            Assert.Equal("./Index", redirectResult?.PageName);
        }

        [Fact]
        public async Task OnPostAsync_IfNotSave_RemainsOnPage()
        {
            var controller = new CreateModel(context, logger, emailSender.Object);
            var loan = new LoanProspect
            {
                IsSave = false,
                Name = "Test name",
                InterestRate = .05,
                LoanAmount = 10000,
                TermMonths = 36
            };
            controller.LoanProspect = loan;
            var result = (await controller.OnPostAsync() as PageResult);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task TrySaveProspect_WhenNotSave_ReturnsFalse()
        {
            var create = new CreateModel(context, logger, emailSender.Object);
            create.LoanProspect.IsSave = false;

            var trySave = await create.TrySaveProspect();

            Assert.False(trySave);
            emailSender.Verify(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            Assert.Equal(0, context.SaveCount);
        }

        [Fact]
        public async Task TrySaveProspect_WhenSave_DoesntEmailIfNoneSpecified()
        {
            var create = new CreateModel(context, logger, emailSender.Object);
            create.LoanProspect.IsSave = true;
            create.LoanProspect.Name = "test person";
            create.LoanProspect.ParseName();
            create.LoanProspect.Email = string.Empty;

            var trySave = await create.TrySaveProspect();
            Assert.True(trySave);
            emailSender.Verify(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            Assert.Equal(1, context.SaveCount);
        }

        [Fact]
        public async Task TrySaveProspect_WhenSave_EmailIfSpecified()
        {
            var create = new CreateModel(context, logger, emailSender.Object);
            var sendTo = "jimw@slalom.com";
            create.LoanProspect.IsSave = true;
            create.LoanProspect.Name = "test person";
            create.LoanProspect.ParseName();
            create.LoanProspect.Email = sendTo;

            var trySave = await create.TrySaveProspect();
            Assert.True(trySave);
            emailSender.Verify(s => s.SendEmailAsync(sendTo, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.Equal(1, context.SaveCount);
        }
    }
}
