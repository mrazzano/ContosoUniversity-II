using System.Net;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Data.Entity.Infrastructure;
using ContosoUniversity.Core.Models;
using ContosoUniversity.Core.Persistence;

namespace ContosoUniversity.Controllers
{
    public class CourseController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CourseController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: Course
        public async Task<ActionResult> Index(int? departmentId)
        {
            await PopulateDepartmentDropDown(departmentId);

            var courses = departmentId.HasValue
                ? await _unitOfWork.Course.GetByDepartmentIdAsync(departmentId.Value)
                : await _unitOfWork.Course.GetAsync();

            return View(courses);
        }

        // GET: Course/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (!id.HasValue)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var course = await _unitOfWork.Course.GetAsync(id.Value);
            if (course == null)
                return HttpNotFound();

            return View(course);
        }

        public async Task<ActionResult> Create()
        {
            await PopulateDepartmentDropDown(null);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "CourseId,Title,Credits,DepartmentId")]Course course)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _unitOfWork.Course.Add(course);
                    await _unitOfWork.Complete();
                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }

            await PopulateDepartmentDropDown(course.DepartmentId);
            return View(course);
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (!id.HasValue)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var course = await _unitOfWork.Course.GetAsync(id.Value);
            if (course == null)
                return HttpNotFound();

            await PopulateDepartmentDropDown(course.DepartmentId);
            return View(course);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditPost(int? id)
        {
            if (!id.HasValue)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var course = await _unitOfWork.Course.GetAsync(id.Value);
            var fieldsToBind = new[] { "Title", "Credits", "DepartmentId" };
            if (TryUpdateModel(course, fieldsToBind))
            {
                try
                {
                    _unitOfWork.Course.Update(course);
                    await _unitOfWork.Complete();
                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }

            await PopulateDepartmentDropDown(course.DepartmentId);
            return View(course);
        }

        // GET: Course/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (!id.HasValue)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var course = await _unitOfWork.Course.GetAsync(id.Value);
            if (course == null)
                return HttpNotFound();

            return View(course);
        }

        // POST: Course/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                _unitOfWork.Course.Delete(id);
                await _unitOfWork.Complete();
                return RedirectToAction("Index");
            }
            catch (RetryLimitExceededException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }
            var course = await _unitOfWork.Course.GetAsync(id);
            return View(course);
        }

        private async Task PopulateDepartmentDropDown(int? departmentId)
        {
            ViewBag.DepartmentId = new SelectList(await _unitOfWork.Department.GetAsync(), "DepartmentId", "Name", departmentId);
        }
    }
}
