using System;
using System.Net;
using System.Linq;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using ContosoUniversity.Core.Models;
using ContosoUniversity.Core.Persistence;
using ContosoUniversity.Core.ViewModels;

namespace ContosoUniversity.Controllers
{
    public class InstructorController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public InstructorController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: Instructor
        public async Task<ActionResult> Index(int? id)
        {
            var viewModel = new InstructorDetailViewModel
            {
                Instructors = await _unitOfWork.Instructor.GetAsync()
            };

            if (!id.HasValue)
                return View(viewModel);

            ViewBag.InstructorId = id.Value;
            viewModel.Courses = viewModel.Instructors.Single(i => i.Id == id.Value).Courses;

            return View(viewModel);
        }

        // GET: Instructor/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (!id.HasValue)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var instructor = await _unitOfWork.Instructor.GetAsync(id.Value);
            if (instructor == null)
                return HttpNotFound();

            return View(instructor);
        }

        public async Task<ActionResult> Create()
        {
            var instructor = new Instructor();
            await PopulateAssignedCourseData(instructor);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "LastName,FirstMidName,HireDate,OfficeAssignment")]Instructor instructor, string[] selectedCourses)
        {
            if (ModelState.IsValid)
            {
                if (selectedCourses != null)
                {

                    foreach (var courseId in selectedCourses.Select(int.Parse))
                    {
                        instructor.Courses.Add(await _unitOfWork.Course.GetAsync(courseId));
                    }
                }
                try
                {
                    _unitOfWork.Instructor.Add(instructor);
                    await _unitOfWork.Complete();
                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }

            await PopulateAssignedCourseData(instructor);
            return View(instructor);
        }

        // GET: Instructor/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (!id.HasValue)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var instructor = await _unitOfWork.Instructor.GetAsync(id.Value);
            if (instructor == null)
                return HttpNotFound();

            await PopulateAssignedCourseData(instructor);
            return View(instructor);
        }

        // POST: Instructor/Edit/5
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditPost(int? id, string[] selectedCourses)
        {
            if (!id.HasValue)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var instructor = await _unitOfWork.Instructor.GetAsync(id.Value);
            var fieldsToBind = new[] { "LastName", "FirstMidName", "HireDate", "OfficeAssignment" };
            if (TryUpdateModel(instructor, fieldsToBind))
            {
                await UpdateInstructorCourses(instructor, selectedCourses);
                try
                {
                    _unitOfWork.Instructor.Update(instructor);
                    await _unitOfWork.Complete();
                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }

            await PopulateAssignedCourseData(instructor);
            return View(instructor);
        }

        // GET: Instructor/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (!id.HasValue)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var instructor = await _unitOfWork.Instructor.GetAsync(id.Value);
            if (instructor == null)
                return HttpNotFound();

            return View(instructor);
        }

        // POST: Instructor/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var department = await _unitOfWork.Department.GetByInstructorIdAsync(id);
                if (department != null)
                {
                    department.InstructorId = null;
                    _unitOfWork.Department.Update(department);
                }
                _unitOfWork.Instructor.Delete(id);
                await _unitOfWork.Complete();
                return RedirectToAction("Index");
            }
            catch (RetryLimitExceededException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }
            var instructor = await _unitOfWork.Instructor.GetAsync(id);
            return View(instructor);
        }

        private async Task PopulateAssignedCourseData(Instructor instructor)
        {
            var allCourses = await _unitOfWork.Course.GetAsync();
            var instructorCourses = new HashSet<int>(instructor.Courses.Select(c => c.CourseId));
            var viewModel = allCourses.Select(course => new AssignedCourseViewModel
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Assigned = instructorCourses.Contains(course.CourseId)
            }).ToList();

            ViewBag.Courses = viewModel;
        }

        private async Task UpdateInstructorCourses(Instructor entity, IReadOnlyCollection<string> courses)
        {
            if (courses == null || courses.Count == 0)
            {
                entity.Courses.Clear();
                return;
            }

            var selectedCourses = new HashSet<int>(courses.Select(c => Convert.ToInt32(c)));
            var instructorCourses = new HashSet<int>(entity.Courses.Select(c => c.CourseId));

            foreach (var course in await _unitOfWork.Course.GetAsync())
            {
                if (selectedCourses.Contains(course.CourseId))
                {
                    if (!instructorCourses.Contains(course.CourseId))
                    {
                        entity.Courses.Add(course);
                    }
                }
                else
                {
                    if (instructorCourses.Contains(course.CourseId))
                    {
                        entity.Courses.Remove(course);
                    }
                }
            }
        }
    }
}
