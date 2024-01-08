//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Gs3PLv9MOBAPI.Models;
//using Gs3PLv9MOBAPI.Services;

//namespace Gs3PLv9MOBAPI.Controllers
//{

//    [ApiController]
//    [Route("api/[controller]")]
//    public class PlaceInfoController : ControllerBase
//    {
//        private readonly IPlaceInfoService _placeInfoService;
//        public PlaceInfoController(IPlaceInfoService placeInfoService)
//        {
//            _placeInfoService = placeInfoService;
//        }
//        // GET api/placeinfo
//        [HttpGet]
//        public IEnumerable<PlaceInfo> GetPlaceInfos() =>_placeInfoService.GetAll();

//        // GET api/placeinfo/id
//        [HttpGet("{id}", Name = nameof(GetPlaceInfoById))]
//        public IActionResult GetPlaceInfoById(int id)
//        {
//            PlaceInfo placeInfo = _placeInfoService.Find(id);
//            if (placeInfo == null) 
//                return NotFound(); 
//            else 
//                return new ObjectResult(placeInfo);
//        }

//        // POST api/placeinfo
//        [HttpPost]
//        public IActionResult PostPlaceInfo([FromBody]PlaceInfo placeinfo)
//        {
//            if (placeinfo == null) return BadRequest();            
//            int retVal=_placeInfoService.Add(placeinfo);            
//            if (retVal > 0) return Ok(); else return NotFound();
//        }
//        // PUT api/placeinfo/guid
//        [HttpPut("{id}")]
//        public IActionResult PutPlaceInfo(int id,[FromBody] PlaceInfo placeinfo)
//        {
//            if (placeinfo == null || id != placeinfo.Id) return BadRequest();
//            if (_placeInfoService.Find(id) == null) return NotFound();
//            int retVal = _placeInfoService.Update(placeinfo);
//            if (retVal > 0) return Ok(); else return NotFound();            
//        }

//        // DELETE api/placeinfo/5
//        [HttpDelete("{id}")]
//        public IActionResult Delete(int id)
//        {
//            int retVal=_placeInfoService.Remove(id);
//            if (retVal > 0) return Ok(); else return NotFound();
//        }
//    }
//}
