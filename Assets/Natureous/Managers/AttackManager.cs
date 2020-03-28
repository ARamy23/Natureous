using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Natureous
{
    public class AttackManager : Singleton<AttackManager>
    {
        public List<AttackInfo> CurrentAttacks = new List<AttackInfo>();
    }
}