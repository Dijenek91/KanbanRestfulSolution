using AutoMapper;
using KanbanModel.DTOs.RequestDTOs;
using KanbanModel.DTOs.ResponseDTOs;
using KanbanModel.ModelClasses;

namespace KanbanModel.DTOs.Mapping
{
    public class TaskProfile : Profile
    {
        public TaskProfile()
        {
            //if any properties are missing in the DTO, set default values
            CreateMap<CreateKanbanTaskRequest, KanbanTask>()
               .ForMember(dest => dest.Description,
                   opt => opt.MapFrom(src => src.Description ?? string.Empty))

               .ForMember(dest => dest.Size,
                   opt => opt.MapFrom(src => src.Size ?? 0))

               .ForMember(dest => dest.PriorityEnum,
                   opt => opt.MapFrom(src => src.PriorityEnum ?? PriorityEnum.Low));

            //all properties are required in full update DTO
            CreateMap<FullUpdateKanbanTaskRequest, KanbanTask>();

            //Only map this property if the source value is NOT null
            CreateMap<PartialUpdateKanbanTaskRequest, KanbanTask>()
                .ForAllMembers(opts =>
                    opts.Condition((src, dest, srcMember) =>
                        srcMember != null
                    ));

            CreateMap<KanbanTask, KanbanTaskResponse>();
        }
    }
}
