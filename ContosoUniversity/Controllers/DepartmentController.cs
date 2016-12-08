using System.Net;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Data.Entity.Infrastructure;
using ContosoUniversity.Core.Models;
using ContosoUniversity.Core.Persistence;

namespace ContosoUniversity.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;


        public DepartmentController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: Department
        public async Task<ActionResult> Index()
        {
            var departments = await _unitOfWork.Department.GetAsync();
            return View(departments);
        }

        // GET: Department/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (!id.HasValue)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var department = await _unitOfWork.Department.GetAsync(id.Value);
            if (department == null)
                return HttpNotFound();

            return View(department);
        }

        // GET: Department/Create
        public async Task<ActionResult> Create()
        {
            await PopulateInstructorDropdown(null);
            return View();
        }

        // POST: Department/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "DepartmentId,Name,Budget,StartDate,InstructorId")] Department department)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _unitOfWork.Department.Add(department);
                    await _unitOfWork.Complete();
                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }

            await PopulateInstructorDropdown(department.InstructorId);
            return View(department);
        }

        // GET: Department/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (!id.HasValue)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var department = await _unitOfWork.Department.GetAsync(id.Value);
            if (department == null)
                return HttpNotFound();

            await PopulateInstructorDropdown(department.InstructorId);
            return View(department);
        }

        // POST: Department/Edit/5
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditPost(int? id)
        {
            if (!id.HasValue)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var department = await _unitOfWork.Department.GetAsync(id.Value);
            string[] fieldsToBind = { "Name", "Budget", "StartDate", "InstructorId", };
            if (TryUpdateModel(department, fieldsToBind))
            {
                try
                {
                    _unitOfWork.Department.Update(department);
                    await _unitOfWork.Complete();
                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }

            await PopulateInstructorDropdown(department.InstructorId);
            return View(department);
        }

        // GET: Department/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var department = await _unitOfWork.Department.GetAsync(id.Value);
            if (department == null)
                return HttpNotFound();

            return View(department);
        }

        // POST: Department/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                _unitOfWork.Department.Delete(id);
                await _unitOfWork.Complete();
                return RedirectToAction("Index");
            }
            catch (RetryLimitExceededException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }
            var department = await _unitOfWork.Department.GetAsync(id);
            return View(department);
        }

        private async Task PopulateInstructorDropdown(int? instructorId)
        {
            ViewBag.InstructorId = new SelectList(await _unitOfWork.Instructor.GetAsync(), "Id", "FullName", instructorId);
        }
    }
}
