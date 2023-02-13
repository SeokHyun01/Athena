using Athena_DataAccess;
using Athena_DataAccess.ViewModel;
using Athena_Models;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena_Business.Mapper
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			CreateMap<Camera, CameraDTO>().ReverseMap();
			CreateMap<EventHeader, EventHeaderDTO>().ReverseMap();
			CreateMap<EventBody, EventBodyDTO>().ReverseMap();
			CreateMap<Event, EventDTO>().ReverseMap();
		}
	}
}
