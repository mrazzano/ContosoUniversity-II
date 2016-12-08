using System.Linq;
using System.Web.Mvc;
using System.Threading.Tasks;
using ContosoUniversity.Core.Persistence;
using ContosoUniversity.Core.ViewModels;

namespace ContosoUniversity.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> Enrollment()
        {
            var students = await _unitOfWork.Student.GetAsync();
            var enrollments = students
              .GroupBy(x => x.EnrollmentDate)
              .Select(g => new EnrollmentDateViewModel
              {
                  EnrollmentDate = g.Key, 
                  StudentCount = g.Count()
              }).ToList();

            return View(enrollments);
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
    }
}