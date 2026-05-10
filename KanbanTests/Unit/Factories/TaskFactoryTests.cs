using AutoMapper;
using KanbanModel.DTOs.ResponseDTOs;
using KanbanModel.ModelClasses;
using KanbanRestService.Factories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using NUnit.Framework;

namespace KanbanTests.Unit.Factories
{
    [TestFixture]
    [Category("Unit")]
    internal class TaskFactoryTests
    {
        private IMapper _mapper = null!;
        private TaskDTOFactory _factory = null!;
        private Mock<IUrlHelper> _urlMock = null!;

        [SetUp]
        public void SetUp()
        {
            // Create a strict mock for IMapper to avoid depending on MapperConfiguration constructor
            var mapperMock = new Mock<IMapper>(MockBehavior.Strict);

            // Map KanbanTask -> KanbanTaskResponse: preserve Id and Name and initialize Links
            mapperMock
                .Setup(m => m.Map<KanbanTaskResponse>(It.IsAny<KanbanTask>()))
                .Returns((KanbanTask? src) =>
                {
                    if (src == null) return (KanbanTaskResponse?)null;
                    return new KanbanTaskResponse
                    {
                        Id = src.Id,
                        Name = src.Name,
                        Description = src.Description,
                        Status = src.Status,
                        Size = src.Size,
                        PriorityEnum = src.PriorityEnum,
                        Links = new List<LinkDTO>()
                    };
                });

            _mapper = mapperMock.Object;
            _factory = new TaskDTOFactory(_mapper);
            _urlMock = new Mock<IUrlHelper>(MockBehavior.Strict);
        }

        [Test]
        public void CreateFoundTaskWithHateoas_throws_ArgumentNullException_when_url_is_null()
        {
            var task = new KanbanTask { Id = 1, Name = "t" };

            Assert.That(() => _factory.CreateFoundTaskWithHateoas(1, task, null!, "http"),
                Throws.ArgumentNullException);
        }

        [Test]
        public void CreateFoundTaskWithHateoas_throws_ArgumentNullException_when_task_is_null()
        {
            Assert.That(() => _factory.CreateFoundTaskWithHateoas(1, null!, _urlMock.Object, "http"),
                Throws.ArgumentNullException);
        }

        [Test]
        public void CreateFoundTaskWithHateoas_CreatesExpectedLinks_PreservesMapping()
        {
            var id = 7;
            var task = new KanbanTask { Id = id, Name = "MyTask" };

            _urlMock.Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                                                                ctx.Action == "GetById" &&
                                                                ctx.Controller == "Tasks" &&
                                                                ctx.Protocol == "https")))
                                                                .Returns("/tasks/7");
            _urlMock.Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                                                                ctx.Action == "EditFullUpdate" &&
                                                                ctx.Controller == "Tasks" &&
                                                                ctx.Protocol == "https")))
                                                                .Returns("/tasks/7/edit");
            _urlMock.Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                                                                ctx.Action == "EditPartialUpdate" &&
                                                                ctx.Controller == "Tasks" &&
                                                                ctx.Protocol == "https")))
                                                                .Returns("/tasks/7/partial");
            _urlMock.Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                                                                ctx.Action == "Delete" &&
                                                                ctx.Controller == "Tasks" &&
                                                                ctx.Protocol == "https")))
                                                                .Returns("/tasks/7/delete");

            var dto = _factory.CreateFoundTaskWithHateoas(id, task, _urlMock.Object, "https");

            Assert.That(dto, Is.Not.Null);
            Assert.That(dto.Id, Is.EqualTo(id));
            Assert.That(dto.Links, Has.Count.EqualTo(4));
            Assert.That(dto.Links.Select(l => l.Rel), Is.EquivalentTo(new[] { "self", "update", "partial update", "delete" }));
            Assert.That(dto.Links.Any(l => l.Href == "/tasks/7"), Is.True);
            Assert.That(dto.Links.Any(l => l.Href == "/tasks/7/edit"), Is.True);
            Assert.That(dto.Links.Any(l => l.Href == "/tasks/7/partial"), Is.True);
            Assert.That(dto.Links.Any(l => l.Href == "/tasks/7/delete"), Is.True);

            _urlMock.VerifyAll();
        }

        [Test]
        public void CreateListFoundTasksWithHateoas_maps_each_item_and_calls_inner_create()
        {
            var tasks = new List<KanbanTask?> { new KanbanTask { Id = 1 }, new KanbanTask { Id = 2 } };

            // set up url actions for both ids
            _urlMock.Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                                                                ctx.Action == "GetById" &&
                                                                ctx.Controller == "Tasks" &&
                                                                ctx.Protocol == "http")))
                                                                .Returns((string?)null);
            _urlMock.Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                                                               ctx.Action == "EditFullUpdate" &&
                                                               ctx.Controller == "Tasks" &&
                                                               ctx.Protocol == "http")))
                                                               .Returns((string?)null);
            _urlMock.Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                                                                ctx.Action == "EditPartialUpdate" &&
                                                                ctx.Controller == "Tasks" &&
                                                                ctx.Protocol == "http")))
                                                                .Returns((string?)null);
            _urlMock.Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                                                                ctx.Action == "Delete" &&
                                                                ctx.Controller == "Tasks" &&
                                                                ctx.Protocol == "http")))
                                                                .Returns((string?)null);
           

            var result = _factory.CreateListFoundTasksWithHateoas(tasks, _urlMock.Object, "http");

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void CreateListFoundTaskWithHateoas_throws_ArgumentNullException_when_task_is_null()
        {
            Assert.That(() => _factory.CreateListFoundTasksWithHateoas(null, _urlMock.Object, "http"),
                Throws.ArgumentNullException);
        }

        [Test]
        public void CreatePagedResult_WithHateoasLinksFor_throws_on_null_inputs()
        {
            Assert.That(() => _factory.CreatePagedResult_WithHateoasLinksFor(null!, null, 0, 10, null, _urlMock.Object, "http"),
                Throws.ArgumentNullException);
            var list = new List<KanbanTaskResponse>();
            Assert.That(() => _factory.CreatePagedResult_WithHateoasLinksFor(list, null, 0, 10, null, null!, "http"),
                Throws.ArgumentNullException);
        }

        [Test]
        public void CreatePagedResult_WithHateoasLinksFor_creates_paging_links_and_metadata()
        {
            var expectedPageNum = 0;
            var expectedPageSize = 2;

            var items = new List<KanbanTaskResponse>
            {
                new KanbanTaskResponse { Id = 1, Links = new List<LinkDTO>() },
                new KanbanTaskResponse { Id = 2, Links = new List<LinkDTO>() }
            };

            // specific page matches first so they are not swallowed by the general GetAll setup
            _urlMock.Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                                                                ctx.Action == "GetAll" &&
                                                                ctx.Controller == "Tasks" &&
                                                                ctx.Protocol == "http" &&
                                                                MatchPageInRouteValues(ctx.Values, 1))))
                                                                .Returns("/tasks?page=1");
            _urlMock.Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                                                                ctx.Action == "GetAll" &&
                                                                ctx.Controller == "Tasks" &&
                                                                ctx.Protocol == "http" &&
                                                                MatchPageInRouteValues(ctx.Values, 0))))
                                                                .Returns("/tasks?page=0");
            // general GetAll (no page-specific route-values)
            _urlMock.Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                                                                ctx.Action == "GetAll" &&
                                                                ctx.Controller == "Tasks" &&
                                                                ctx.Protocol == "http"))).Returns("/tasks");
            // Create with null route-values
            _urlMock.Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                                                                ctx.Action == "Create" &&
                                                                ctx.Controller == "Tasks" &&
                                                                ctx.Protocol == "http" &&
                                                                ctx.Values == null)))
                                                                .Returns("/tasks");

            var paged = _factory.CreatePagedResult_WithHateoasLinksFor(items, null, expectedPageNum, expectedPageSize, null, _urlMock.Object, "http");

            Assert.That(paged, Is.Not.Null);
            Assert.That(paged.TotalCount, Is.EqualTo(2));
            Assert.That(paged.Page, Is.EqualTo(expectedPageNum));
            Assert.That(paged.Size, Is.EqualTo(expectedPageSize));
            Assert.That(paged.Links.Select(l => l.Rel), Is.EquivalentTo(new[] { "self", "create", "next", "prev" }));
        }

        // helpers to inspect anonymous routeValues objects
        private static bool MatchPageInRouteValues(object valuesObj, int expectedPage)
        {
            if (valuesObj == null) return false;
            var props = valuesObj.GetType().GetProperties();
            var pageProp = props.FirstOrDefault(p => p.Name.Equals("page", System.StringComparison.OrdinalIgnoreCase));
            if (pageProp == null) return false;
            var val = pageProp.GetValue(valuesObj);
            return val != null && val.Equals(expectedPage);
        }
    }
}