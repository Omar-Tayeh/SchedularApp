namespace SchedularCalendar.Helpers
{
    public static class JSONHelper
    {
        public static string GetEventListJSONString(List<Models.Event> events)
        {
            var eventList = new List<Event>();
            foreach (var model in events)
            {
                var myEvent = new Event()
                {
                    id = model.Id,
                    title = model.Title,
                    start = model.StartDate,
                    end = model.EndDate.AddDays(1),
                    description = model.Description,
                    creator = model.CreatorId
                };
                eventList.Add(myEvent);
            }
            return System.Text.Json.JsonSerializer.Serialize(eventList);
        }


        public class Event
        {
            public int id { get; set; }
            public string title { get; set; }
            public DateTime start { get; set; }
            public DateTime end { get; set; }
            public bool allDay { get; set; }
            public string description { get; set; }
            public string creator { get; set; }

        }
    }
}
