using Assets.Scripts.BaseScripts;
using Assets.Scripts.Helpers;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    /// <summary>
    /// Контроллер ввода
    /// </summary>
    public class InputController : BaseController
    {
        private RaycastHit rayInfo;
        private LayerMask mask;

        public InputController(LayerMask Mask)
        {
            mask = Mask;
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
            
            if(Physics.Raycast(Ray, out rayInfo, 200f, mask))
            {
                rayInfo.transform.gameObject.GetComponent<Cell>().OnClick(Click);
            }
        }
    }
}
