﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena_Models
{
	public class ObjectDetectionResponseDTO
	{
		public IEnumerable<PredictionResultDTO?> Results { get; set; }
		public string ErrorMessage { get; set; }
	}
}
