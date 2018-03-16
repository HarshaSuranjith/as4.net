import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { SortablejsModule } from 'angular-sortablejs';

import { As4ComponentsModule } from '../common';
import { AuthenticationModule } from './../authentication/authentication.module';
import { RuntimeModule } from './../runtime/runtime.module';
import { AgentSettingsComponent } from './agent/agent.component';
import { BaseSettingsComponent } from './base.component';
import { CommonSettingsComponent } from './commonsettings.component';
import { DatabaseSettingsComponent } from './database.component';
import { PortalSettingsComponent } from './portalsettings/portalsettings.component';
import { ReceiverComponent } from './receiver.component';
import { ReceptionAwarenessAgentComponent } from './receptionawarenessagent/receptionawarenessagent.component';
import { RuntimeService } from './runtime.service';
import { RuntimeStore } from './runtime.store';
import { ROUTES } from './settings.routes';
import { SettingsService } from './settings.service';
import { AuthorizationMapComponent } from './authorizationmap/authorizationmap.component';
import { AuthorizationMapService } from './authorizationmap/authorizationmapservice';
import { TransformerComponent } from './transformer.component';
import { SettingsStore } from './settings.store';
import { SettingsComponent } from './settings/settings.component';
import { SmpConfigurationComponent } from './smpconfiguration/smpconfiguration.component';
import { StepSettingsComponent } from './step/step.component';

const components: any = [
    SettingsComponent,
    BaseSettingsComponent,
    CommonSettingsComponent,
    DatabaseSettingsComponent,
    AgentSettingsComponent,
    ReceiverComponent,
    TransformerComponent,
    StepSettingsComponent,
    ReceptionAwarenessAgentComponent,
    PortalSettingsComponent,
    AuthorizationMapComponent,
    SmpConfigurationComponent
];

const services: any = [
    SettingsService,
    RuntimeService,
    SettingsStore,
    RuntimeStore,
    AuthorizationMapService
];

@NgModule({
    declarations: [
        ...components
    ],
    providers: [
        ...services
    ],
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        RouterModule.forChild(ROUTES),
        SortablejsModule,
        AuthenticationModule,
        As4ComponentsModule,
        RuntimeModule
    ],
    exports: [
        SettingsComponent,
        BaseSettingsComponent,
        CommonSettingsComponent,
        DatabaseSettingsComponent,
        AgentSettingsComponent,
        ReceiverComponent
    ]
})
export class SettingsModule {
}
