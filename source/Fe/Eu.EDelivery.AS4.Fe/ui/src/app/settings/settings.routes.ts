import { Routes } from '@angular/router';

import { MustBeAuthorizedGuard } from '../common/mustbeauthorized.guard';
import { CanDeactivateGuard } from './../common/candeactivate.guard';
import { WrapperComponent } from './../common/wrapper.component';
import { AgentSettingsComponent } from './agent/agent.component';
import { PortalSettingsComponent } from './portalsettings/portalsettings.component';
import { SettingsComponent } from './settings/settings.component';
import { SmpConfigurationComponent } from './smpconfiguration/smpconfiguration.component';

export const ROUTES: Routes = [
  {
    path: 'submit',
    component: WrapperComponent,
    children: [
      {
        path: '',
        component: AgentSettingsComponent,
        data: {
          title: 'Submit Agents',
          type: 'submitAgents',
          icon: 'fa-cloud-upload',
          weight: -10,
          betype: 0
        },
        canActivate: [MustBeAuthorizedGuard],
        canDeactivate: [CanDeactivateGuard]
      }
    ]
  },
  {
    path: 'receive',
    component: WrapperComponent,
    children: [
      {
        path: 'push',
        component: AgentSettingsComponent,
        data: {
          title: 'Push Receive Agents',
          type: 'receiveAgents',
          icon: 'fa-cloud-download',
          betype: 1
        },
        canActivate: [MustBeAuthorizedGuard],
        canDeactivate: [CanDeactivateGuard]
      },
      {
        path: 'pull',
        component: AgentSettingsComponent,
        data: {
          title: 'Pull Receive Agents',
          type: 'pullReceiveAgents',
          icon: 'fa-cloud-download',
          betype: 6
        },
        canActivate: [MustBeAuthorizedGuard],
        canDeactivate: [CanDeactivateGuard]
      }
    ],
    data: { title: 'Receive Agents', icon: 'fa-cloud-download', weight: -9 },
    canActivate: [MustBeAuthorizedGuard],
    canDeactivate: [CanDeactivateGuard]
  },
  {
    path: 'settings',
    component: WrapperComponent,
    children: [
      {
        path: '',
        redirectTo: 'portal',
        pathMatch: 'full',
        canDeactivate: [CanDeactivateGuard]
      },
      {
        path: 'portal',
        component: PortalSettingsComponent,
        data: { title: 'Portal settings' },
        canDeactivate: [CanDeactivateGuard]
      },
      {
        path: 'runtime',
        component: SettingsComponent,
        data: { title: 'Runtime settings' },
        canDeactivate: [CanDeactivateGuard]
      },
      {
        path: 'agents',
        data: { title: 'Internal Agents' },
        children: [
          {
            path: '',
            redirectTo: 'submit',
            pathMatch: 'full',
            canDeactivate: [CanDeactivateGuard]
          },
          {
            path: 'outboundprocessing',
            component: AgentSettingsComponent,
            data: {
              title: 'Outbound processing',
              header: 'Outbound processing agent',
              type: 'outboundProcessingAgents',
              betype: 8,
              showwarning: true
            },
            canDeactivate: [CanDeactivateGuard]
          },
          {
            path: 'send',
            component: AgentSettingsComponent,
            data: {
              title: 'Send',
              header: 'Send agent',
              type: 'sendAgents',
              betype: 2,
              showwarning: true
            },
            canDeactivate: [CanDeactivateGuard]
          },
          {
            path: 'deliver',
            component: AgentSettingsComponent,
            data: {
              title: 'Deliver',
              header: 'Deliver agent',
              type: 'deliverAgents',
              betype: 3,
              showwarning: true
            },
            canDeactivate: [CanDeactivateGuard]
          },
          {
            path: 'notify',
            component: AgentSettingsComponent,
            data: {
              title: 'Notify',
              header: 'Notify agent',
              type: 'notifyAgents',
              betype: 4,
              showwarning: true
            },
            canDeactivate: [CanDeactivateGuard]
          },
          {
            path: 'pullsend',
            component: AgentSettingsComponent,
            data: {
              title: 'Pull send',
              header: 'Pull send agent',
              type: 'pullSendAgents',
              betype: 7,
              showwarning: true
            },
            canDeactivate: [CanDeactivateGuard]
          }
        ]
      }
    ],
    data: { title: 'Settings', icon: 'fa-toggle-on' },
    canActivate: [MustBeAuthorizedGuard],
    canDeactivate: [CanDeactivateGuard]
  },
  {
    path: 'smpconfiguration',
    component: WrapperComponent,
    canActivate: [MustBeAuthorizedGuard],
    children: [
      {
        path: '',
        component: SmpConfigurationComponent,
        data: { title: 'Smp configuration' },
        canDeactivate: [CanDeactivateGuard]
      }
    ]
  }
];
