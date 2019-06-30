using Assets.Scripts.BaseScripts;
using Assets.Scripts.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Controllers
{
    /// <summary>
    /// Контроллер ввода
    /// </summary>
    public class InputController : BaseController
    {
        private RaycastHit RayInfo;
        private LayerMask Mask;

        public InputController(LayerMask Mask)
        {
            this.Mask = Mask;
        }

        public override void ControllerUpdate()
        {
            if(Input.GetMouseButtonDown(0))
            {
                CheckCell(MouseClick.LeftButton);
            }
            else if(Input.GetMouseButtonDown(1))
            {
                CheckCell(MouseClick.RightButton);
            }
        }

        /// <summary>
        /// Метод наведения на ячейку
        /// </summary>
        /// <param name="Click"></param>
        private void CheckCell(MouseClick Click)
        {
            Ray Ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if(Physics.Raycast(Ray, out RayInfo, 200f, Mask))
            {
                RayInfo.transform.gameObject.GetComponent<Cell>().OnClick(Click);
            }
        }
    }
}
