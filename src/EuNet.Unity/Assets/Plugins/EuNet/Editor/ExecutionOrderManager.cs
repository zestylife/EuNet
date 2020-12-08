using System;
using UnityEditor;
using EuNet.Unity;

[InitializeOnLoad]
public class ExecutionOrderManager
{
    static ExecutionOrderManager()
    {
        foreach (MonoScript monoScript in MonoImporter.GetAllRuntimeMonoScripts())
        {
            if (monoScript.GetClass() != null)
            {
                foreach (var a in Attribute.GetCustomAttributes(monoScript.GetClass(), typeof(ExecutionOrder)))
                {
                    var currentOrder = MonoImporter.GetExecutionOrder(monoScript);
                    var newOrder = ((ExecutionOrder)a).Order;
                    if (currentOrder != newOrder)
                        MonoImporter.SetExecutionOrder(monoScript, newOrder);

                    //Debug.Log(string.Format("{0} {1}", monoScript.GetClass().Name, newOrder));
                }
            }
        }
    }
}
