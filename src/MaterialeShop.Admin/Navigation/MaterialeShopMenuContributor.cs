using MaterialeShop.Localization;
using MaterialeShop.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Account.Localization;
using Volo.Abp.AuditLogging.Blazor.Menus;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Identity.Pro.Blazor.Navigation;
using Volo.Abp.LanguageManagement.Blazor.Menus;
using Volo.Abp.OpenIddict.Pro.Blazor.Menus;
using Volo.Abp.SettingManagement.Blazor.Menus;
using Volo.Abp.TextTemplateManagement.Blazor.Menus;
using Volo.Abp.UI.Navigation;
using Volo.Saas.Host.Blazor.Navigation;

namespace MaterialeShop.Admin.Navigation
{
    public class MaterialeShopMenuContributor : IMenuContributor
    {
        private readonly IConfiguration _configuration;

        public MaterialeShopMenuContributor(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task ConfigureMenuAsync(MenuConfigurationContext context)
        {
            if (context.Menu.Name == StandardMenus.Main)
            {
                await ConfigureMainMenuAsync(context);
            }
            else if (context.Menu.Name == StandardMenus.User)
            {
                await ConfigureUserMenuAsync(context);
            }
        }

        private static Task ConfigureMainMenuAsync(MenuConfigurationContext context)
        {
            var l = context.GetLocalizer<MaterialeShopResource>();

            context.Menu.AddItem(new ApplicationMenuItem(
                MaterialeShopMenus.Home,
                l["Menu:Home"],
                "/",
                icon: "fas fa-home",
                order: 1
            ));

            //HostDashboard
            context.Menu.AddItem(
                new ApplicationMenuItem(
                    MaterialeShopMenus.HostDashboard,
                    l["Menu:Dashboard"],
                    "/HostDashboard",
                    icon: "fa fa-chart-line",
                    order: 2
                ).RequirePermissions(MaterialeShopPermissions.Dashboard.Host)
            );

            //TenantDashboard
            context.Menu.AddItem(
                new ApplicationMenuItem(
                    MaterialeShopMenus.TenantDashboard,
                    l["Menu:Dashboard"],
                    "/Dashboard",
                    icon: "fa fa-chart-line",
                    order: 2
                ).RequirePermissions(MaterialeShopPermissions.Dashboard.Tenant)
            );
            /* Example nested menu definition:

            context.Menu.AddItem(
                new ApplicationMenuItem("Menu0", "Menu Level 0")
                .AddItem(new ApplicationMenuItem("Menu0.1", "Menu Level 0.1", url: "/test01"))
                .AddItem(
                    new ApplicationMenuItem("Menu0.2", "Menu Level 0.2")
                        .AddItem(new ApplicationMenuItem("Menu0.2.1", "Menu Level 0.2.1", url: "/test021"))
                        .AddItem(new ApplicationMenuItem("Menu0.2.2", "Menu Level 0.2.2")
                            .AddItem(new ApplicationMenuItem("Menu0.2.2.1", "Menu Level 0.2.2.1", "/test0221"))
                            .AddItem(new ApplicationMenuItem("Menu0.2.2.2", "Menu Level 0.2.2.2", "/test0222"))
                        )
                        .AddItem(new ApplicationMenuItem("Menu0.2.3", "Menu Level 0.2.3", url: "/test023"))
                        .AddItem(new ApplicationMenuItem("Menu0.2.4", "Menu Level 0.2.4", url: "/test024")
                            .AddItem(new ApplicationMenuItem("Menu0.2.4.1", "Menu Level 0.2.4.1", "/test0241"))
                    )
                    .AddItem(new ApplicationMenuItem("Menu0.2.5", "Menu Level 0.2.5", url: "/test025"))
                )
                .AddItem(new ApplicationMenuItem("Menu0.2", "Menu Level 0.2", url: "/test02"))
            );

            */

            context.Menu.SetSubItemOrder(SaasHostMenus.GroupName, 3);

            //Administration
            var administration = context.Menu.GetAdministration();
            administration.Order = 3;

            //Administration->Identity
            administration.SetSubItemOrder(IdentityProMenus.GroupName, 1);

            //Administration->OpenId
            administration.SetSubItemOrder(OpenIddictProMenus.GroupName, 2);

            //Administration->Language Management
            administration.SetSubItemOrder(LanguageManagementMenus.GroupName, 3);

            //Administration->Text Template Management
            administration.SetSubItemOrder(TextTemplateManagementMenus.GroupName, 4);

            //Administration->Audit Logs
            administration.SetSubItemOrder(AbpAuditLoggingMenus.GroupName, 5);

            //Administration->Settings
            administration.SetSubItemOrder(SettingManagementMenus.GroupName, 6);

            context.Menu.AddItem(
                new ApplicationMenuItem(
                    MaterialeShopMenus.Listas,
                    l["Menu:Listas"],
                    url: "/listas",
                    icon: "fa fa-list",
                    requiredPermissionName: MaterialeShopPermissions.Listas.Default)
            );

            context.Menu.AddItem(
                new ApplicationMenuItem(
                    MaterialeShopMenus.ListaItems,
                    l["Menu:ListaItems"],
                    url: "/lista-items",
                    icon: "fa fa-list",
                    requiredPermissionName: MaterialeShopPermissions.ListaItems.Default)
            );

            context.Menu.AddItem(
                new ApplicationMenuItem(
                    MaterialeShopMenus.Enderecos,
                    l["Menu:Enderecos"],
                    url: "/enderecos",
                    icon: "fa fa-map-marker-alt",
                    requiredPermissionName: MaterialeShopPermissions.Enderecos.Default)
            );
            return Task.CompletedTask;
        }

        private async Task ConfigureUserMenuAsync(MenuConfigurationContext context)
        {
            var accountStringLocalizer = context.GetLocalizer<AccountResource>();
            var authServerUrl = _configuration["AuthServer:Authority"] ?? "";

            context.Menu.AddItem(new ApplicationMenuItem(
                "Account.Manage",
                accountStringLocalizer["MyAccount"],
                $"{authServerUrl.EnsureEndsWith('/')}Account/Manage?returnUrl={_configuration["App:SelfUrl"]}",
                icon: "fa fa-cog",
                order: 1000,
                null).RequireAuthenticated());

            context.Menu.AddItem(new ApplicationMenuItem(
                "Account.SecurityLogs",
                accountStringLocalizer["MySecurityLogs"],
                $"{authServerUrl.EnsureEndsWith('/')}Account/SecurityLogs?returnUrl={_configuration["App:SelfUrl"]}",
                icon: "fa fa-cog",
                order: 1001,
                null).RequireAuthenticated());

            await Task.CompletedTask;
        }
    }
}