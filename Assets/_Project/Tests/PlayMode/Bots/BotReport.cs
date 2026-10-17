using System.Collections.Generic;
using System.Text;

namespace DecirQueNo.Tests.Bots
{
    public class BotReport
    {
        public string Bot;
        public Ending Ending;
        public int FinalFollowers;
        public int FinalHealth;
        public int GoalFollowers;
        public int Timeouts;
        public readonly List<DaySummary> Days = new List<DaySummary>();

        public int Yes { get { int n = 0; foreach (DaySummary d in Days) n += d.Yes; return n; } }
        public int Maybe { get { int n = 0; foreach (DaySummary d in Days) n += d.Maybe; return n; } }
        public int No { get { int n = 0; foreach (DaySummary d in Days) n += d.No; return n; } }
        public int Expired { get { int n = 0; foreach (DaySummary d in Days) n += d.Expired; return n; } }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"[BotReport] {Bot}: ending={Ending} followers={FinalFollowers}/{GoalFollowers} health={FinalHealth} ");
            sb.Append($"days={Days.Count} yes={Yes} maybe={Maybe} no={No} expired={Expired} timeouts={Timeouts}");
            foreach (DaySummary d in Days)
            {
                sb.Append($"\n  day {d.Day}: yes={d.Yes} maybe={d.Maybe} no={d.No} expired={d.Expired} followers{d.FollowersDelta:+#,0;-#,0;+0} health{d.HealthDelta:+#;-#;+0}");
            }

            return sb.ToString();
        }
    }
}
