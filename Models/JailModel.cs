using JailPlugin.Models.Sub_Models;
using Newtonsoft.Json;
using Rocket.Unturned.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace JailPlugin.Models
{
    public class JailModel
    {
        [JsonIgnore]
        public GameObject Object { get; set; }

        public string JailName { get; set; }
        public SVector3 JailPosition { get; set; }
        public float WalkRadius { get; set; }
        public int MaxPlayers { get; set; }
        public List<JailSession> Sessions { get; set; }
        public SVector3 ReturnPosition { get; set; }
        public JailModel()
        {

        }
        public JailModel(string jailName, SVector3 jailPosition, float walkRadius, int maxPlayers, List<JailSession> sessions, SVector3 returnPosition)
        {
            JailName = jailName;
            JailPosition = jailPosition;
            WalkRadius = walkRadius;
            MaxPlayers = maxPlayers;
            Sessions = sessions;
            ReturnPosition = returnPosition;
        }

        public void SetupColissors()
        {
            if (Main.Instance.Configuration.Instance.ImpedirPlayerDeEscapar)
            {
                Object = new GameObject(JailName);
                Object.SetActive(true);
                Object.layer = 30;
                Object.transform.position = JailPosition.ToVector3();

                Object.AddComponent<Rigidbody>();
                Rigidbody Body = Object.GetComponent<Rigidbody>();
                Body.isKinematic = true;
                Body.useGravity = false;

                Object.AddComponent(typeof(SphereCollider));
                SphereCollider Col = Object.GetComponent<SphereCollider>();
                Col.radius = WalkRadius;
                Col.isTrigger = true;

                DetectPlayerTresspassing Script = Object.AddComponent<DetectPlayerTresspassing>();
                Script.Jail = this;
            }
        }
        public void RemoveColissors()
        {
            if (Main.Instance.Configuration.Instance.ImpedirPlayerDeEscapar)
            {
                GameObject.Destroy(Object);
            }
        }
        public void RestartColissors()
        {
            if (Main.Instance.Configuration.Instance.ImpedirPlayerDeEscapar)
            {
                RemoveColissors();
                SetupColissors();
            }
        }
    }
}
