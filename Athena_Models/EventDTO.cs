﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena_Models
{
	public class EventDTO
	{
		public EventHeaderDTO EventHeader { get; set; }
		public IEnumerable<EventBodyDTO?> EventBodies { get; set; }
	}
}
