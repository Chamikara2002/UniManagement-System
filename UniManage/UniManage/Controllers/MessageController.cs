using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using UniManage.Services;
using UniManage.ViewModels;

namespace UniManage.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly MessageService _messageService;

        public MessageController()
        {
            _messageService = new MessageService();
        }

        public async Task<ActionResult> Inbox()
        {
            var userId = User.Identity.GetUserId();
            var list = await _messageService.GetInboxAsync(userId);
            return View(list);
        }

        public async Task<ActionResult> Sent()
        {
            var userId = User.Identity.GetUserId();
            var list = await _messageService.GetSentAsync(userId);
            return View(list);
        }

        public async Task<ActionResult> Details(int id)
        {
            var userId = User.Identity.GetUserId();
            var msg = await _messageService.GetMessageAsync(id, userId);
            if (msg == null) return HttpNotFound();
            if (msg.ToUserId == userId && !msg.IsRead) await _messageService.MarkAsReadAsync(id, userId);
            return View(msg);
        }

        public async Task<ActionResult> Compose()
        {
            // Determine potential recipients based on role
            var userId = User.Identity.GetUserId();
            if (User.IsInRole("Student"))
            {
                var recipients = await _messageService.GetPotentialRecipientsForStudentAsync(userId);
                ViewBag.Recipients = new SelectList(recipients, "Id", "FullName");
            }
            else if (User.IsInRole("Lecturer"))
            {
                var recipients = await _messageService.GetPotentialRecipientsForLecturerAsync(userId);
                ViewBag.Recipients = new SelectList(recipients, "Id", "FullName");
            }
            else
            {
                var recipients = await _messageService.GetAllUsersAsync();
                ViewBag.Recipients = new SelectList(recipients, "Id", "FullName");
            }

            return View(new ComposeMessageViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Compose(ComposeMessageViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.Identity.GetUserId();
            // validate recipient rules
            var (allowed, message) = await _messageService.ValidateRecipientForSenderAsync(userId, model.ToUserId);
            if (!allowed)
            {
                ModelState.AddModelError("ToUserId", message ?? "Invalid recipient");
                return View(model);
            }

            await _messageService.SendMessageAsync(userId, model);
            TempData["Success"] = "Message sent.";
            return RedirectToAction("Sent");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _messageService?.Dispose();
            base.Dispose(disposing);
        }
    }
}
