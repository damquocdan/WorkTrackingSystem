﻿namespace WorkTrackingSystem.Areas.EmployeeSystem.Models
{
    public class UpdateProgressRequest
    {
        public long Id {  get; set; }
        public long? JobRepeatId { get; set; }
        public double? Progress {  get; set; }
    }
}
