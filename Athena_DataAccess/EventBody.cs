﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena_DataAccess
{
	public class EventBody
	{
		[Key]
		public int Id { get; set; }
		public string Label { get; set; }
		public int Left { get; set; }
		public int Right { get; set; }
		public int Top { get; set; }
		public int Bottom { get; set; }
		public int EventHeaderId { get; set; }
		[ForeignKey(nameof(EventHeaderId))]
		public EventHeader EventHeader { get; set; }
	}
}
