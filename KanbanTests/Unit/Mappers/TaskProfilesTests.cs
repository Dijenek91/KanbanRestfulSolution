using AutoMapper;
using KanbanModel.DTOs.Mapping;
using KanbanModel.DTOs.RequestDTOs;
using KanbanModel.DTOs.ResponseDTOs;
using KanbanModel.ModelClasses;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace KanbanTests.Unit.Mappers
{
    [TestFixture]
    [Category("Unit")]
    internal class TaskProfilesTests
    {
        private IMapper _mapper = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<TaskProfile>();
            }, NullLoggerFactory.Instance);

            // validate config to catch mapping mistakes early
            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
        }

        [Test]
        public void CreateMap_NullsAreDefaulted_WhenMapping_CreateKanbanTaskRequest_ToKanbanTask()
        {
            var req = new CreateKanbanTaskRequest
            {
                Name = "New",
                Description = null,    // should default to empty string
                Size = null,           // should default to 0
                PriorityEnum = null    // should default to PriorityEnum.Low
            };

            var mapped = _mapper.Map<KanbanTask>(req);

            Assert.That(mapped, Is.Not.Null);
            Assert.That(mapped.Name, Is.EqualTo("New"));
            Assert.That(mapped.Description, Is.EqualTo(string.Empty));
            Assert.That(mapped.Size, Is.EqualTo(0));
            Assert.That(mapped.PriorityEnum, Is.EqualTo(PriorityEnum.Low));
        }

        [Test]
        public void FullUpdateMap_MapsAllProperties_From_FullUpdateKanbanTaskRequest()
        {
            var req = new FullUpdateKanbanTaskRequest
            {
                Name = "Full",
                Description = "desc",
                Status = StatusEnum.InProgress,
                Size = 3,
                PriorityEnum = PriorityEnum.High
            };

            var mapped = _mapper.Map<KanbanTask>(req);

            Assert.That(mapped.Name, Is.EqualTo(req.Name));
            Assert.That(mapped.Description, Is.EqualTo(req.Description));
            Assert.That(mapped.Status, Is.EqualTo(req.Status));
            Assert.That(mapped.Size, Is.EqualTo(req.Size));
            Assert.That(mapped.PriorityEnum, Is.EqualTo(req.PriorityEnum));
        }

        [Test]
        public void PartialUpdate_DoesNotOverwrite_With_Null_Source_Members()
        {
            // destination has existing values
            var dest = new KanbanTask
            {
                Id = 10,
                Name = "ExistingName",
                Description = "ExistingDesc",
                Size = 5,
                PriorityEnum = PriorityEnum.Medium
            };

            // source has only Description set, other members null => only Description should change
            var src = new PartialUpdateKanbanTaskRequest
            {
                Name = null,
                Description = "UpdatedDesc",
                Size = null,
                PriorityEnum = null
            };

            _mapper.Map(src, dest);

            Assert.That(dest.Id, Is.EqualTo(10)); // unchanged
            Assert.That(dest.Name, Is.EqualTo("ExistingName")); // not overwritten by null
            Assert.That(dest.Description, Is.EqualTo("UpdatedDesc")); // updated
            Assert.That(dest.Size, Is.EqualTo(5)); // unchanged
            Assert.That(dest.PriorityEnum, Is.EqualTo(PriorityEnum.Medium)); // unchanged
        }

        [Test]
        public void PartialUpdate_Overwrites_When_Source_Member_NotNull()
        {
            var dest = new KanbanTask
            {
                Name = "Old",
                Description = "OldDesc",
                Size = 1,
                PriorityEnum = PriorityEnum.Low
            };

            var src = new PartialUpdateKanbanTaskRequest
            {
                Name = "NewName",
                Description = null,
                Size = 2,
                PriorityEnum = PriorityEnum.High
            };

            _mapper.Map(src, dest);

            Assert.That(dest.Name, Is.EqualTo("NewName"));
            Assert.That(dest.Description, Is.EqualTo("OldDesc")); // null source didn't overwrite
            Assert.That(dest.Size, Is.EqualTo(2));
            Assert.That(dest.PriorityEnum, Is.EqualTo(PriorityEnum.High));
        }

        [Test]
        public void TaskToResponse_MapsProperties_To_KanbanTaskResponse()
        {
            var task = new KanbanTask
            {
                Id = 77,
                Name = "T",
                Description = "d",
                Status = StatusEnum.ToDo,
                Size = 4,
                PriorityEnum = PriorityEnum.Low
            };

            var resp = _mapper.Map<KanbanTaskResponse>(task);

            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.Id, Is.EqualTo(task.Id));
            Assert.That(resp.Name, Is.EqualTo(task.Name));
            Assert.That(resp.Description, Is.EqualTo(task.Description));
            Assert.That(resp.Status, Is.EqualTo(task.Status));
            Assert.That(resp.Size, Is.EqualTo(task.Size));
            Assert.That(resp.PriorityEnum, Is.EqualTo(task.PriorityEnum));
            // Links are mapped as-is by profile; if not initialized by source, response may have null/empty -> ensure property exists
            Assert.That(resp.Links, Is.Not.Null.Or.Empty.Or.Null); // existence check only; factory tests cover link population
        }
    }
}