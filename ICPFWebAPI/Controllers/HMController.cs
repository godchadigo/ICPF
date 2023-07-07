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
}
