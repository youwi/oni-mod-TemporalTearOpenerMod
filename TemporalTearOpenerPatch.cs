using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Database;
using HarmonyLib;
using UnityEngine;

namespace oni_mod_test
{
    //可拆可建
    [HarmonyPatch(typeof(TemporalTearOpenerConfig))]
    public class Class1Patch
    {
        [HarmonyPatch("DoPostConfigureComplete")]
        public static void Postfix(  GameObject  go )
        {
            go.GetComponent<Deconstructable>().allowDeconstruction = true;
            EstablishColonies.BASE_COUNT = 1;//强制修改数量为0;
        }

    }
    [HarmonyPatch(typeof(TemporalTearOpener.Instance))]
    public class TpOPatch
    {
        [HarmonyPatch("OpenTemporalTear")]
        public static void Postfix(TemporalTearOpener.Instance __instance)

        {
            int openerWorldId = __instance.GetComponent<StateMachineController>().GetMyWorldId();//这个好像只能获取主星的ID
            ClusterManager.Instance.GetClusterPOIManager().OpenTemporalTear(openerWorldId);

            FieldInfo fld = typeof(TemporalTearOpener.Instance).GetField("charging"); //初始化
            if (fld != null)
            {
                var charging = fld.GetValue(__instance);
                __instance.GoTo((StateMachine.BaseState)charging);
             }
 

        //    FieldInfo fld_op = typeof(TemporalTearOpener.Instance).GetField("opening_tear_beam"); //opening_tear_beam
         //   var opening_tear_beam = fld_op.GetValue(__instance);
         //   __instance.GoTo((StateMachine.BaseState)opening_tear_beam);

            FieldInfo fld_mc = typeof(TemporalTearOpener.Instance).GetField("m_particlesConsumed");
            if (fld_mc != null)
            {
                fld_mc.SetValue(__instance, 0f);//点击开火,数据清0;
            }

            ClusterManager.Instance.GetWorld(openerWorldId).GetSMI<GameplaySeasonManager.Instance>().StartNewSeason(Db.Get().GameplaySeasons.TemporalTearMeteorShowers);
            //清理粒子
            HighEnergyParticleStorage highEnergyParticleStorage = __instance.GetComponent<HighEnergyParticleStorage>();
            if(highEnergyParticleStorage!=null)
                 highEnergyParticleStorage.ConsumeAll();
           
  
        }
    }
   // [HarmonyPatch(typeof(TemporalTearOpener))]
    public class TemporalTearOpenerInitPatch
    {
   //     [HarmonyPatch("InitializeStates")]

        public static void Postfix(TemporalTearOpener __instance)
        {
          

            __instance.root.Enter(delegate (TemporalTearOpener.Instance smi)
            {

              //  smi.UpdateMeter();
                FieldInfo fld = typeof(TemporalTearOpener).GetField("charging"); //初始化
                FieldInfo ready_fld = typeof(TemporalTearOpener).GetField("ready"); //初始化
                FieldInfo check_fld = typeof(TemporalTearOpener).GetField("check_requirements"); //初始化

                //check_requirements
                if (fld != null)
                {
 
                    var charging = (GameStateMachine<TemporalTearOpener, TemporalTearOpener.Instance, IStateMachineTarget, TemporalTearOpener.Def>.State)fld.GetValue(__instance);
                  
                 //   smi.GoTo((StateMachine.BaseState)check_fld.GetValue(__instance));

                    //添加触发器.满了进入ready状态.
                    var ready = (GameStateMachine<TemporalTearOpener, TemporalTearOpener.Instance, IStateMachineTarget, TemporalTearOpener.Def>.State)ready_fld.GetValue(__instance);
                  /*  ready.EventTransition(GameHashes.OnParticleStorageChanged,
                       ready,
                         delegate (TemporalTearOpener.Instance smi2)  {
                             //直接触发陨石,如果满的话.
                             HighEnergyParticleStorage highEnergyParticleStorage = smi2.GetComponent<HighEnergyParticleStorage>();
                             int openerWorldId = smi2.GetComponent<StateMachineController>().GetMyWorldId();//这个好像只能获取主星的ID
                             if (smi2.GetComponent<HighEnergyParticleStorage>().IsFull())
                             {
                                 highEnergyParticleStorage.ConsumeAll();
                                 ClusterManager.Instance.GetWorld(openerWorldId).GetSMI<GameplaySeasonManager.Instance>().StartNewSeason(Db.Get().GameplaySeasons.TemporalTearMeteorShowers);
                                 return true;
                             }
                             //清理粒子

                             return false;
                        });
                     */
                }
                /*                fld = typeof(TemporalTearOpener).GetField("check_requirements"); //转检查器.
                                if (fld != null)
                                {
                                    var check_requirements = fld.GetValue(__instance);
                                    smi.GoTo((StateMachine.BaseState)check_requirements);//进入初始状态.
                                }*/
            }).PlayAnim("off");//重新重置动画.
        
        }

    }

    [HarmonyPatch(typeof(TemporalTearOpener.Instance))]
    public class SidescreenEnabledPatch
    {
        [HarmonyPatch("SidescreenEnabled")]
        public static bool Postfix(bool __result, TemporalTearOpener.Instance __instance)
        {
            //  __instance.m
            if (__instance.GetComponent<HighEnergyParticleStorage>().IsFull())
                __result= true;
            //强制显示菜单
            return __result;
        }

    }
    [HarmonyPatch(typeof(TemporalTearOpener.Instance))]
    public class SidescreenButtonInteractablePatch
    {
        [HarmonyPatch("SidescreenButtonInteractable")]
        public static bool Postfix(bool __result, TemporalTearOpener.Instance __instance)
        {
            //  __instance.m
            if (__instance.GetComponent<HighEnergyParticleStorage>().IsFull())
                __result = true;
            //强制显示菜单,强制让按钮可点击.
            return __result;
        }

    }

    //清理多余的陨石
    [HarmonyPatch(typeof(GameplaySeasonManager.Instance))]
    public class GsmPatch
    {
        [HarmonyPatch("Update")]
        public static void Prefix(GameplaySeasonManager.Instance __instance)
        {
            if (__instance.activeSeasons.Count()> 0)
            {
                var obj = __instance.activeSeasons.Last();
                __instance.activeSeasons.Clear();
                __instance.activeSeasons.Add(obj);
            }
      
        }
    }


    // copy /Y "C:/Users/amd/source/repos/oni-mod-test/bin/Debug/oni-mod-test.dll"   "D:/Doc/Klei/OxygenNotIncluded\mods/local/OpenTest/oni-mod-test.dll" 
 
    //添加建筑到菜单中
    [HarmonyPatch(typeof(GeneratedBuildings))]
    [HarmonyPatch("LoadGeneratedBuildings")]
    public class ImplementationPatch
    { 
        private static void Prefix()
        {

            ModUtil.AddBuildingToPlanScreen("Base", "TemporalTearOpener");
        }

    }


}
