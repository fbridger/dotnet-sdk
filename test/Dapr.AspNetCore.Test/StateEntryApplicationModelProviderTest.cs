﻿// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Dapr.AspNetCore.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Dapr.AspNetCore.Resources;
    using Dapr.Client;
    using Dapr.Client.Autogen.Grpc.v1;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public class StateEntryApplicationModelProviderTest
    {
        [Fact]
        public void OnProvidersExecuted_NullActionsBindingSource()
        {
            var provider = new StateEntryApplicationModelProvider();
            var context = CreateContext(nameof(ApplicationModelProviderTestController.Get));

            Action action = () => provider.OnProvidersExecuted(context);

            action
                .Should()
                .NotThrow<NullReferenceException>();
        }

        [Fact]
        public void OnProvidersExecuted_StateEntryParameterThrows()
        {
            var provider = new StateEntryApplicationModelProvider();
            var context = CreateContext(nameof(ApplicationModelProviderTestController.Post));

            Action action = () => provider.OnProvidersExecuted(context);

            action
                .Should()
                .Throw<InvalidOperationException>(SR.ErrorStateStoreNameNotProvidedForStateEntry);
        }

        private ApplicationModelProviderContext CreateContext(string methodName)
        {
            var controllerType = typeof(ApplicationModelProviderTestController).GetTypeInfo();
            var typeInfoList = new List<TypeInfo> { controllerType };

            var context = new ApplicationModelProviderContext(typeInfoList);
            var controllerModel = new ControllerModel(controllerType, new List<object>(0));

            context.Result.Controllers.Add(controllerModel);

            var methodInfo = controllerType.AsType().GetMethods().First(m => m.Name.Equals(methodName));
            var actionModel = new ActionModel(methodInfo, controllerModel.Attributes)
            {
                Controller = controllerModel
            };

            controllerModel.Actions.Add(actionModel);
            var parameterInfo = actionModel.ActionMethod.GetParameters().First();
            var parameterModel = new ParameterModel(parameterInfo, controllerModel.Attributes)
            {
                BindingInfo = new BindingInfo(),
                Action = actionModel,
            };

            actionModel.Parameters.Add(parameterModel);

            return context;
        }

        [Controller]
        private class ApplicationModelProviderTestController : Controller
        {
            [HttpGet]
            public void Get([Bind(Prefix = "s")]int someId) { }

            [HttpPost]
            public void Post(StateEntry<Subscription> bogusEntry) { }
        }
    }
}
