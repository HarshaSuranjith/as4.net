import { Observer } from 'rxjs/Observer';
import { Observable } from 'rxjs/Observable';
import {
    inject,
    TestBed
} from '@angular/core/testing';
import { Component } from '@angular/core';
import {
    BaseRequestOptions,
    ConnectionBackend,
    Http
} from '@angular/http';
import { MockBackend } from '@angular/http/testing';

import { ActivatedRoute } from '@angular/router';
import { FormBuilder } from '@angular/forms';

import { ItemType } from './../../api/ItemType';
import { SettingsAgent } from './../../api/SettingsAgent';
import { SettingsAgents } from './../../api/SettingsAgents';
import { Settings } from './../../api/Settings';
import { AgentSettingsComponent } from './agent.component';
import { SettingsStore } from '../settings.store';
import { SettingsService } from '../settings.service';
import { SettingsServiceMock } from '../settings.service.mock';
import { DialogService } from './../../common/dialog.service';
import { RuntimeServiceMock } from '../runtime.service.mock';
import { RuntimeStore } from '../runtime.store';
import { RuntimeService } from '../runtime.service';
import { ModalService } from '../../common/modal/modal.service';

describe('agent', () => {
    const currentAgentName: string = 'currentAgent';
    let currentAgent: SettingsAgent;
    let otherAgent: SettingsAgent;
    let settings: Settings;
    let agents: Array<SettingsAgent>;
    beforeEach(() => {
        currentAgent = new SettingsAgent();
        currentAgent.name = currentAgentName;
        otherAgent = new SettingsAgent();
        otherAgent.name = 'otherAgent';
        agents = new Array<SettingsAgent>();
        agents.push(currentAgent);
        agents.push(otherAgent);

        settings = new Settings();
        settings.agents = new SettingsAgents();
        settings.agents.sendAgents = agents;
    });
    beforeEach(() => TestBed.configureTestingModule({
        providers: [
            AgentSettingsComponent,
            SettingsStore,
            { provide: SettingsService, useClass: SettingsServiceMock },
            { provide: RuntimeService, useClass: RuntimeServiceMock },
            {
                provide: ActivatedRoute,
                useValue: {
                    snapshot: {
                        data: {
                            type: 'sendAgents',
                            title: 'test'
                        }
                    }
                }
            },
            FormBuilder,
            RuntimeStore,
            DialogService,
            ModalService
        ]
    }));
    it('should use the title property from the activatedRoute snapshot', inject([AgentSettingsComponent, RuntimeStore], (agent: AgentSettingsComponent) => {
        expect(agent.title).toBe('test agent');
    }));
    it('should use the activatedRoute snapshot title property when settingsStore has a state', inject([AgentSettingsComponent, SettingsStore], (agent: AgentSettingsComponent, settingsStore: SettingsStore) => {
        // Act
        settingsStore.setState({ Settings: settings });

        // Assert
        expect(agent.agent).toBe('sendAgents');
    }));
    it('settings should be loaded when settingStore pushes', inject([AgentSettingsComponent, SettingsStore], (agent: AgentSettingsComponent, settingsStore: SettingsStore) => {
        // Act
        settingsStore.setState({ Settings: settings });

        // Assert
        expect(agent.settings).toBe(settings.agents.sendAgents);
        expect(agent.form).toBeDefined();
    }));
    it('should set transformers when runtimeStore pushhes', inject([AgentSettingsComponent, RuntimeStore], (agent: AgentSettingsComponent, runtimeStore: RuntimeStore) => {
        // Setup
        let transformers = new Array<ItemType>();

        // Act
        runtimeStore.setState({
            receivers: new Array<ItemType>(),
            steps: new Array<ItemType>(),
            transformers: transformers,
            certificateRepositories: new Array<ItemType>(),
            deliverSenders: new Array<ItemType>(),
            runtimeMetaData: new Array<any>()
        });

        // Assert
        expect(agent.transformers).toBe(transformers);
    }));
    it('should reselect the currentAgent after store event', inject([AgentSettingsComponent, SettingsStore], (agent: AgentSettingsComponent, settingsStore: SettingsStore) => {
        // Setup
        agent.settings = settings.agents.sendAgents;
        settingsStore.setState({ Settings: settings });
        agent.selectAgent(currentAgent.name);

        // Act
        settingsStore.setState({ Settings: settings });

        // Assert
        expect(agent.currentAgent.name).toBe(currentAgent.name);
    }));
    it('should be disabled when no agent is selected', inject([AgentSettingsComponent], (agent: AgentSettingsComponent) => {
        // Assert
        expect(agent.form.disabled).toBeTruthy();

        agent.currentAgent = currentAgent;
        expect(agent.form.disabled).toBeFalsy();

        agent.currentAgent = null;
        expect(agent.form.disabled).toBeTruthy();
    }));

    describe('addAgent', () => {
        it('should ask user for an agent name when he wants to create a new agent and should use the correct actionType', inject([AgentSettingsComponent, ModalService], (agent: AgentSettingsComponent, ModalService: ModalService) => {
            const newAgentName = 'NEWAGENT';
            let dialogSpy = spyOn(ModalService, 'show').and.returnValue(Observable.of(true));

            // Request a custom (= empty new agent)
            agent.actionType = -1;
            agent.newName = newAgentName;
            agent.addAgent();
            expect(dialogSpy).toHaveBeenCalled();
            expect(agent.currentAgent.name).toBe(newAgentName);
            expect(agent.form.value.name).toBe(newAgentName);
            expect(agent.currentAgent.receiver).toBeUndefined();
            expect(agent.form.dirty).toBeTruthy();

            agent.reset();

            // Request actionType = first agent (= clone agent except for the name)
            let firstAgent = agents[0];
            agent.actionType = firstAgent.name;
            agent.newName = newAgentName;
            agent.addAgent();
            expect(dialogSpy).toHaveBeenCalled()
            expect(agent.currentAgent.name).toBe(newAgentName);
            expect(agent.form.value.name).toBe(newAgentName);
            expect(agent.currentAgent.receiver).toBe(firstAgent.receiver);
            expect(agent.form.dirty).toBeTruthy();
        }));
        it('should only allow one agent with the same name', inject([AgentSettingsComponent, ModalService, DialogService, SettingsStore], (agent: AgentSettingsComponent, service: ModalService, dialogService: DialogService, store: SettingsStore) => {
            store.setState({ Settings: settings });
            agent.selectAgent(currentAgent.name);
            agent.newName = currentAgent.name;
            let dialogSpy = spyOn(service, 'show').and.returnValue(Observable.of(currentAgent.name));
            let existsSpy = spyOn(dialogService, 'message').and.returnValue(Observable.of(true));
            let agentCount = agent.settings.length;

            agent.addAgent();

            expect(agent.settings.length).toEqual(agentCount);
            expect(existsSpy).toHaveBeenCalledWith(`An agent with the name ${agent.newName} already exists`);
        }));
    });

    describe('selectAgent', () => {
        it('if the form is dirty then user should confirm and the currentAgent should be changed', inject([AgentSettingsComponent, DialogService], (agent: AgentSettingsComponent, dialogService: DialogService) => {
            agent.settings = agents;

            let dialogSpy = spyOn(dialogService, 'confirmUnsavedChanges').and.returnValue(Observable.of(true));

            agent.currentAgent = currentAgent;
            agent.form.markAsDirty();

            // Act
            agent.selectAgent(otherAgent.name);

            // Assert
            expect(agent.currentAgent === otherAgent).toBeTruthy();
        }));
        it('currentAgent should not be changed when user doesnt confirm when form is dirty', inject([AgentSettingsComponent, DialogService], (agent: AgentSettingsComponent, dialogService: DialogService) => {
            agent.settings = agents;
            agent.currentAgent = currentAgent;

            let dialogSpy = spyOn(dialogService, 'confirmUnsavedChanges').and.returnValue(Observable.of(false));
            agent.form.markAsDirty();

            // Act
            agent.selectAgent(otherAgent.name);

            // Assert
            expect(agent.currentAgent === currentAgent).toBeTruthy();
            expect(agent.isNewMode).toBeFalsy();
        }));

        it('should prompt the user for a new and remove the old one from the list', inject([AgentSettingsComponent, DialogService], (agent: AgentSettingsComponent, dialogService: DialogService) => {
            agent.settings = agents;
            agent.currentAgent = currentAgent;
            agent.isNewMode = true;
            agent.form.markAsDirty();

            let dialogSpy = spyOn(dialogService, 'confirmUnsavedChanges').and.returnValue(Observable.of(true));

            // Act
            agent.selectAgent(otherAgent.name);

            // Assert
            expect(agent.settings.find(agt => agt === currentAgent)).toBeUndefined();
        }));
    });

    describe('save', () => {
        it('should show a message when the form is invalid', inject([AgentSettingsComponent, SettingsService, DialogService], (agent: AgentSettingsComponent, settingsService: SettingsService, dialogService: DialogService) => {
            let form = {
                valid: false
            };

            agent.form = <any>form;
            let dialogSpy = spyOn(dialogService, 'message');

            // Act
            agent.save();

            // Assert
            expect(dialogSpy).toHaveBeenCalled();
            expect(form.valid).toBeFalsy();
        }));
        it('should call updateAgent when the form is valid and not in newMode', inject([AgentSettingsComponent, SettingsService, DialogService, SettingsStore], (agent: AgentSettingsComponent, settingsService: SettingsService, dialogService: DialogService, settingsStore: SettingsStore) => {
            settingsStore.setState({ Settings: settings });
            let form = {
                valid: true,
                value: currentAgent,
                markAsPristine: () => { },
                enable: () => { }
            };
            agent.form = <any>form;
            agent.isNewMode = false;
            agent.currentAgent = currentAgent;

            let formPristineSpy = spyOn(form, 'markAsPristine');
            let settingsSpy = spyOn(settingsService, 'updateAgent').and.returnValue(Observable.of(true));

            // Act
            agent.save();

            // Assert
            expect(settingsSpy).toHaveBeenCalled();
            expect(settingsSpy).toHaveBeenCalledWith(currentAgent, currentAgent.name, 'sendAgents');
            expect(formPristineSpy).toHaveBeenCalled();
        }));
        it('should call createAgent when the form is valid and in newMode', inject([AgentSettingsComponent, SettingsService, DialogService], (agent: AgentSettingsComponent, settingsService: SettingsService, dialogService: DialogService) => {
            let form = {
                valid: true,
                value: currentAgent,
                markAsPristine: () => { }
            };
            agent.form = <any>form;
            agent.isNewMode = true;

            let formPristineSpy = spyOn(form, 'markAsPristine');
            let settingsSpy = spyOn(settingsService, 'createAgent').and.returnValue(Observable.of(true));

            // Act
            agent.save();

            // Assert
            expect(settingsSpy).toHaveBeenCalled();
            expect(settingsSpy).toHaveBeenCalledWith(currentAgent, 'sendAgents');
            expect(formPristineSpy).toHaveBeenCalled();
        }));
    });

    describe('reset', () => {
        it('should remove the currentAgent if in new mode', inject([AgentSettingsComponent], (agent: AgentSettingsComponent) => {
            agent.isNewMode = true;
            agent.currentAgent = currentAgent;
            agent.settings = agents;

            // Act
            agent.reset();

            // Assert
            expect(agent.settings.find(agt => agt === currentAgent)).toBeUndefined();
        }));
        it('should reset the form to the currentAgent value', inject([AgentSettingsComponent], (agent: AgentSettingsComponent) => {
            agent.isNewMode = false;
            agent.currentAgent = currentAgent;

            // Act
            agent.form.value.name = 'test';
            expect(agent.form.value.name).toBe('test');
            agent.reset();

            // Assert
            expect(agent.form.value.name).toBe(currentAgentName);
        }));
        it('should revert the name back to its original value when it has been renamed', inject([AgentSettingsComponent, DialogService], (agent: AgentSettingsComponent, dialogService: DialogService) => {
            agent.isNewMode = false;
            agent.currentAgent = currentAgent;

            let dialogServiceSpy = spyOn(dialogService, 'prompt').and.returnValue(Observable.of('newName'));

            // Act
            agent.rename();
            expect(agent.form.value.name).toBe('newName');
            agent.reset();

            // Assert
            expect(dialogServiceSpy).toHaveBeenCalled();
            expect(currentAgent.name).toBe(currentAgentName);
        }));
    });
    describe('rename', () => {
        it('should prompt the user for a new name', inject([AgentSettingsComponent, DialogService, SettingsStore], (agent: AgentSettingsComponent, dialogService: DialogService, settingsStore: SettingsStore) => {
            settingsStore.setState({ Settings: settings });
            agent.currentAgent = currentAgent;
            let dialogServiceSpy = spyOn(dialogService, 'prompt').and.returnValue(Observable.of('newName'));
            let formSpy = spyOn(agent.form, 'markAsDirty');

            // Act
            agent.rename();

            // Assert
            expect(dialogServiceSpy).toHaveBeenCalled();
            expect(agent.currentAgent.name).toBe(currentAgentName);
            expect(agent.form.value.name).toBe('newName');
            expect(formSpy).toHaveBeenCalled();
        }));
        it('should not be possible to rename an agent to the same name as another agent', inject([AgentSettingsComponent, DialogService, SettingsStore], (agent: AgentSettingsComponent, dialogService: DialogService, settingsStore: SettingsStore) => {
            let currentName = currentAgent.name;
            settingsStore.setState({ Settings: settings });
            agent.selectAgent(currentAgent.name);
            let dialogSpy = spyOn(dialogService, 'prompt').and.returnValue(Observable.of(otherAgent.name));
            let existsDialogSpy = spyOn(dialogService, 'message').and.returnValue(Observable.of(true));

            agent.rename();

            expect(dialogSpy).toHaveBeenCalled();
            expect(agent.currentAgent.name).toBe(currentName);
            expect(agent.form.value.name).toBe(currentName);
            expect(existsDialogSpy).toHaveBeenCalledWith(`An agent with the name ${otherAgent.name} already exists`);
        }));
    });
    describe('delete', () => {
        it('should ask the user for confirmation', inject([AgentSettingsComponent, DialogService], (agent: AgentSettingsComponent, dialogService: DialogService) => {
            let dialogServiceSpy = spyOn(dialogService, 'confirm').and.returnValue(Observable.of(true));
            agent.currentAgent = currentAgent;

            // Act
            agent.delete();

            // Assert
            expect(dialogServiceSpy).toHaveBeenCalled();
        }));
        it('should do nothing when the uesr cancelled', inject([AgentSettingsComponent, SettingsService, DialogService], (cmp: AgentSettingsComponent, service: SettingsService, dialogService: DialogService) => {
            let dialogSpy = spyOn(dialogService, 'confirm').and.returnValue(Observable.of(false));
            let deleteAgentSpy = spyOn(service, 'deleteAgent');

            cmp.delete();

            expect(dialogSpy).toHaveBeenCalled();
            expect(deleteAgentSpy).not.toHaveBeenCalled();
        }));
        it('should remove the currentAgent when confirmed and in new mode and mark the form as pristine', inject([AgentSettingsComponent, DialogService], (agent: AgentSettingsComponent, dialogService: DialogService) => {
            let dialogServiceSpy = spyOn(dialogService, 'confirm').and.returnValue(Observable.of(true));
            agent.currentAgent = currentAgent;
            agent.settings = agents;
            agent.isNewMode = true;
            agent.form.markAsDirty();
            expect(agent.form.dirty).toBeTruthy();

            // Act
            agent.delete();

            // Assert
            expect(agent.settings.find(agt => agt === currentAgent)).toBeUndefined();
            expect(agent.form.pristine).toBeTruthy();
        }));
        it('should call deleteAgent when confirmed and not in new mdoe', inject([AgentSettingsComponent, DialogService, SettingsService], (agent: AgentSettingsComponent, dialogService: DialogService, settingService: SettingsService) => {
            let dialogServiceSpy = spyOn(dialogService, 'confirm').and.returnValue(Observable.of(true));
            let serviceSpy = spyOn(settingService, 'deleteAgent');
            agent.currentAgent = currentAgent;
            agent.isNewMode = false;

            // Act
            agent.delete();

            // Assert
            expect(serviceSpy).toHaveBeenCalledWith(currentAgent, 'sendAgents');
        }));
    });
});
