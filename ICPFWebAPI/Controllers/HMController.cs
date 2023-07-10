using Microsoft.AspNetCore.Mvc;
using PFC;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ICPFWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HMController : ControllerBase
    {

        private readonly IPFC _pfcService;

        public HMController(IPFC pfcService)
        {
            _pfcService = pfcService;
        }
        // GET: api/<HMController>
        [HttpGet]
        public async Task<OperationModel> Get()
        {
            var mbus = new ReadDataModel()
            {
                DeviceName = "MBUS_1",
                Address = "0",
                ReadLength = 100,
                DatasType = DataType.Int32,
            };
            var mcResult = _pfcService.GetData(mbus);
            return mcResult;
        }
        [HttpGet("/GetChartData/{tagName}")]
        public async Task<ChartModel> GetChartData(string tagName)
        {
            ChartModel model = new ChartModel();
            model.TagName = tagName;
            switch (tagName)
            {
                case "機臺電壓":
                    {
                        //Note:暫時先生成歷史數據用於測試圖表
                        //5760筆一天
                       
                        model.Data.Add("電壓R相", GetData1(220 , 240));
                        model.Data.Add("電壓S相", GetData1(110 , 130));
                        model.Data.Add("電壓T相", GetData1(22 , 26));
                    }
                    break;
                case "機臺電流":
                    break;
                case "機臺瓦時計":
                    break;
            }

            return model;
        }
        public List<DtPoint> GetData1(int min , int max)
        {
            List<DtPoint> data = new List<DtPoint>();

            DateTime startTime = DateTime.Today; // 取得當天的起始時間

            double timeInterval = 24.0 / 5760.0; // 計算每個等分的時間間隔

            Random r = new Random();
            for (int i = 0; i < 5760; i++)
            {
                double time = i * timeInterval;
                TimeSpan timeSpan = TimeSpan.FromHours(time);

                DateTime resultTime = startTime.Add(timeSpan); // 將時間間隔加到起始時間上得到結果時間

                data.Add(new DtPoint() { Value = r.Next(min, max), DateTime = resultTime });
            }
            return data;
        }

        // GET api/<HMController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<HMController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<HMController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<HMController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
    public class ChartModel
    {
        public string TagName { get; set; }
        public Dictionary<string , List<DtPoint>> Data { get; set; } = new Dictionary<string, List<DtPoint>>();
    }
    public class DtPoint
    {
        public int Value { get; set; }
        public DateTime DateTime { get; set; }
    }
}
