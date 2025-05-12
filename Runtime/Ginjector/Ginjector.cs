using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GrimTools.Runtime
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public sealed class GinjectAttribute : PropertyAttribute { }
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class GprovideAttribute : PropertyAttribute { }

    public interface IDenpendcyProvider { }

    [DefaultExecutionOrder(-1000)]
    public class Ginjector : MonoBehaviour
    {
        const BindingFlags _bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        readonly Dictionary<Type, object> registry = new Dictionary<Type, object>();

        private void Awake()
        {
            MonoBehaviour[] monoBehaviours = FindMonoBehaviors();

            // Find all modules implementing IDependencyProvider and register the dependencies they provide
            var providers = monoBehaviours.OfType<IDenpendcyProvider>();
            foreach(var provider in providers)
            {
                Register(provider);
            }

            // Find all injectable objects and inject dependencies
            var injectables = monoBehaviours.Where(IsInjectable);
            foreach(var injectable in injectables)
            {
                Inject(injectable);
            }
        }
        private void Inject(object instance)
        {
            var type = instance.GetType();

            // Inject into fields
            var injectableFields = type.GetFields(_bindingFlags)
                .Where(field => Attribute.IsDefined(field, typeof(GinjectAttribute)));
            foreach (var injectableField in injectableFields)
            {
                if(injectableField.GetValue(instance) != null)
                {
                    Debug.LogWarning($"[Ginjector] Field '{injectableField.Name}' in class '{type.Name}' is already set. Skipping injection.");
                    continue;
                }

                var fieldType = injectableField.FieldType;
                var resolvedInstance = Resolve(fieldType);
                if(resolvedInstance == null)
                {
                    throw new Exception($"Field '{injectableField.Name}' in class '{type.Name}' could not be injected. Type '{fieldType.Name}' not found in registry.");
                }
                injectableField.SetValue(instance, resolvedInstance); 
            }

            // Inject into methods
            var injectableMethods = type.GetMethods(_bindingFlags)
                .Where(method => Attribute.IsDefined(method, typeof(GinjectAttribute)));
            foreach (var injectableMethod in injectableMethods)
            {
                var requiredParameters = injectableMethod.GetParameters()
                                               .Select(param => param.ParameterType)
                                               .ToArray();
                var resolvedInstances = requiredParameters.Select(Resolve).ToArray();
                if(resolvedInstances.Any(resolvedInstance => resolvedInstance == null))
                {
                    throw new Exception($"Failed to inject dependencies into method '{injectableMethod.Name}' in class '{type.Name}'.");
                }
                injectableMethod.Invoke(instance, resolvedInstances);
            }

            // Inject into properties
            var injectableProperties = type.GetProperties(_bindingFlags)
                .Where(property => Attribute.IsDefined(property, typeof(GinjectAttribute)));
            foreach (var injectableProperty in injectableProperties)
            {
                var propertyType = injectableProperty.PropertyType;
                var resolvedInstance = Resolve(propertyType);
                if (resolvedInstance == null)
                {
                    throw new Exception($"Property '{injectableProperty.Name}' in class '{type.Name}' could not be injected. Type '{propertyType.Name}' not found in registry.");
                }

                injectableProperty.SetValue(instance, resolvedInstance);
            }
        }
        private void Register<T>(T instance)
        {
            registry[typeof(T)] = instance;
        }
        private void Register(IDenpendcyProvider provider)
        {
            var methods = provider.GetType().GetMethods(_bindingFlags);

            foreach(var method in methods)
            {
                if (!Attribute.IsDefined(method, typeof(GprovideAttribute))) continue;

                var returnType = method.ReturnType;
                var providedInstance = method.Invoke(provider, null);
                if (providedInstance != null)
                {
                    registry.Add(returnType, providedInstance);
                }
                else throw new Exception($"Provider method '{method.Name}' in class '{provider.GetType().Name} returned null when providing type '{returnType.Name}'");

            }
                
        }
        public void ValidateDependencies()
        {
            var monoBehaviours = FindMonoBehaviors();
            var providers = monoBehaviours.OfType<IDenpendcyProvider>();
            var providedDependencies = GetProvidedDependencies(providers);

            var invalidDependencies = monoBehaviours
                .SelectMany(mb => mb.GetType().GetFields(_bindingFlags), (mb, field) => new { mb, field })
                .Where(t => Attribute.IsDefined(t.field, typeof(GinjectAttribute)))
                .Where(t => !providedDependencies.Contains(t.field.FieldType) && t.field.GetValue(t.mb) == null)
                .Select(t => $"[ValidationLayer] {t.mb.GetType().Name} is missing dependency {t.field.FieldType.Name} on GameObject '{t.mb.gameObject.name}'");

            var invalidDependencyList = invalidDependencies.ToList();
            if(!invalidDependencyList.Any())
            {
                Debug.Log("[ValidationLayer] All dependencies are valid.");
            }
            else
            {
                Debug.LogError($"[ValidationLayer] Found {invalidDependencyList.Count} invalid dependencies:");
                foreach (var invalidDependency in invalidDependencyList)
                {
                    Debug.LogError(invalidDependency);
                }
            }
        }
        HashSet<Type> GetProvidedDependencies(IEnumerable<IDenpendcyProvider> providers)
        {
            var providedDependencies = new HashSet<Type>();
            foreach(var provider in providers)
            {
                var methods = provider.GetType().GetMethods(_bindingFlags);

                foreach(var method in methods)
                {
                    if(!Attribute.IsDefined(method, typeof(GprovideAttribute)))
                        continue;
                    var returnType = method.ReturnType;
                    providedDependencies.Add(returnType);   
                }
            }
            return providedDependencies;
        }
        public void ClearDependencies()
        {
            foreach(var monoBehavior in FindMonoBehaviors())
            {
                var type = monoBehavior.GetType();
                var injectableFields = type.GetFields(_bindingFlags)
                    .Where(field => Attribute.IsDefined(field, typeof(GinjectAttribute)));
                foreach (var injectableField in injectableFields)
                {
                    injectableField.SetValue(monoBehavior, null);
                }
            }
            Debug.Log("[Ginjector] Cleared all injectable fields.");
        }
        private object Resolve(Type type)
        {
            if (registry.TryGetValue(type, out var resolvedInstance))
            {
                return resolvedInstance;
            }
            else
            {
                throw new Exception($"Type '{type.Name}' not registered in Ginjector");
            }
        }
        static bool IsInjectable(MonoBehaviour monoBehaviour)
        {
            var members = monoBehaviour.GetType().GetMembers(_bindingFlags);
            return members.Any(m => Attribute.IsDefined(m, typeof(GinjectAttribute)));
        }
        static MonoBehaviour[] FindMonoBehaviors()
        {
            return FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.InstanceID);
        }
    }
}

