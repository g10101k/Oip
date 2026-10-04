import { DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { ConfirmationService } from 'primeng/api';
import { Button } from 'primeng/button';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { Dialog } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { MultiSelectModule } from 'primeng/multiselect';
import { Select } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { Tab, TabList, TabPanel, TabPanels, Tabs } from 'primeng/tabs';
import { TagModule } from 'primeng/tag';
import { Textarea } from 'primeng/textarea';
import { ToggleSwitch } from 'primeng/toggleswitch';
import { ToolbarModule } from 'primeng/toolbar';
import { Tooltip } from 'primeng/tooltip';
import { BaseModuleComponent, NoSettingsDto, provideTranslations, SecurityComponent } from 'oip-common';
import { AgentModuleApi } from '../../../api/agent-module.api';
import {
  AgentDto,
  AgentModuleSettings,
  AgentToolDto,
  LlmProviderDto,
  SaveAgentRequest,
  SaveSkillRequest,
  SkillDto
} from '../../../api/data-contracts';
import en from './l10n/agent-module.en.json';
import ru from './l10n/agent-module.ru.json';

type AgentModuleTab = 'agents' | 'skills' | 'tools';

interface AgentEditModel {
  code: string;
  name: string;
  description: string;
  systemPrompt: string;
  llmProviderId: number | null;
  isEnabled: boolean;
  skillIds: number[];
}

interface SkillEditModel {
  code: string;
  description: string;
  instructions: string;
  isEnabled: boolean;
  tools: string[];
}

@Component({
  selector: 'app-agent-module',
  providers: [AgentModuleApi, ConfirmationService],
  imports: [
    SecurityComponent,
    TranslatePipe,
    Button,
    ConfirmDialog,
    InputText,
    MultiSelectModule,
    Select,
    Tabs,
    TabList,
    Tab,
    TabPanels,
    TabPanel,
    Textarea,
    ToggleSwitch,
    ToolbarModule,
    Tooltip,
    Dialog,
    TableModule,
    TagModule,
    FormsModule,
    DatePipe
  ],
  template: `
    @if (isContent) {
      <p-confirmDialog />
      <div class="card w-full">
        <div class="mb-4">
          <p-toolbar>
            <div class="flex flex-col md:flex-row md:items-center gap-2 w-full">
              <div class="font-semibold text-lg flex items-center gap-2 mx-2">
                <i class="pi pi-sparkles"></i>
                {{ title }}
              </div>

              <div class="flex-1"></div>
              <div class="flex items-center gap-1 w-full md:w-auto">
                <p-button
                  icon="pi pi-refresh"
                  rounded="true"
                  severity="secondary"
                  text="true"
                  tooltipPosition="bottom"
                  id="oip-agent-module-refresh"
                  [loading]="loading"
                  [disabled]="activeRowAction"
                  [pTooltip]="'agent-module.content.refreshTooltip' | translate"
                  (onClick)="loadAll()"></p-button>
                @if (activeTab !== 'tools') {
                  <p-button
                    icon="pi pi-plus"
                    rounded="true"
                    severity="success"
                    text="true"
                    tooltipPosition="bottom"
                    id="oip-agent-module-add"
                    [disabled]="loading || activeRowAction || !canEdit"
                    [pTooltip]="
                      (activeTab === 'agents' ? 'agent-module.content.addAgent' : 'agent-module.content.addSkill')
                        | translate
                    "
                    (onClick)="activeTab === 'agents' ? beginCreateAgent() : beginCreateSkill()"></p-button>
                }
                <input
                  class="w-full md:w-96"
                  pInputText
                  type="text"
                  id="oip-agent-module-filter"
                  [placeholder]="'agent-module.content.searchPlaceholder' | translate"
                  [(ngModel)]="globalFilter" />
                <p-button
                  icon="pi pi-filter-slash"
                  rounded="true"
                  severity="secondary"
                  text="true"
                  tooltipPosition="bottom"
                  id="oip-agent-module-clear-filter"
                  [disabled]="!globalFilter"
                  [pTooltip]="'agent-module.content.clear' | translate"
                  (onClick)="globalFilter = ''"></p-button>
              </div>
            </div>
          </p-toolbar>
        </div>

        <p-tabs [(value)]="activeTab">
          <p-tablist>
            <p-tab id="oip-agent-module-tab-agents" value="agents">
              <i class="pi pi-sparkles mr-2"></i>{{ 'agent-module.tabs.agents' | translate }}
            </p-tab>
            <p-tab id="oip-agent-module-tab-skills" value="skills">
              <i class="pi pi-book mr-2"></i>{{ 'agent-module.tabs.skills' | translate }}
            </p-tab>
            <p-tab id="oip-agent-module-tab-tools" value="tools">
              <i class="pi pi-wrench mr-2"></i>{{ 'agent-module.tabs.tools' | translate }}
            </p-tab>
          </p-tablist>
          <p-tabpanels>
            <p-tabpanel value="agents">
              <p-table dataKey="id" [paginator]="true" [rows]="50" [loading]="loading" [value]="visibleAgents">
                <ng-template pTemplate="header">
                  <tr>
                    <th pSortableColumn="code">
                      {{ 'agent-module.agents.code' | translate }}
                      <p-sortIcon field="code" />
                    </th>
                    <th pSortableColumn="name">
                      {{ 'agent-module.agents.name' | translate }}
                      <p-sortIcon field="name" />
                    </th>
                    <th>{{ 'agent-module.agents.provider' | translate }}</th>
                    <th>{{ 'agent-module.agents.skills' | translate }}</th>
                    <th class="text-center">{{ 'agent-module.content.status' | translate }}</th>
                    <th pSortableColumn="updatedAt">
                      {{ 'agent-module.content.updatedAt' | translate }}
                      <p-sortIcon field="updatedAt" />
                    </th>
                    <th class="text-center min-w-32">{{ 'agent-module.content.actions' | translate }}</th>
                  </tr>
                </ng-template>

                <ng-template let-agent pTemplate="body">
                  <tr>
                    <td>
                      <span class="font-mono">{{ agent.code }}</span>
                    </td>
                    <td>
                      <div class="font-medium">{{ agent.name }}</div>
                      @if (agent.description) {
                        <div class="text-sm text-surface-500">{{ agent.description }}</div>
                      }
                    </td>
                    <td>{{ providerName(agent.llmProviderId) }}</td>
                    <td>
                      <div class="flex flex-wrap gap-1">
                        @for (code of skillCodes(agent.skillIds); track code) {
                          <p-tag severity="secondary" [value]="code" />
                        }
                      </div>
                    </td>
                    <td class="text-center">
                      <p-tag
                        [severity]="agent.isEnabled ? 'success' : 'danger'"
                        [value]="
                          (agent.isEnabled ? 'agent-module.content.enabled' : 'agent-module.content.disabled')
                            | translate
                        " />
                    </td>
                    <td>{{ agent.updatedAt | date: layoutService.dateTimeFormat() }}</td>
                    <td>
                      <div class="flex items-center justify-center gap-1">
                        <p-button
                          icon="pi pi-pencil"
                          rounded="true"
                          text="true"
                          tooltipPosition="bottom"
                          [disabled]="activeRowAction || !canEdit"
                          [pTooltip]="'agent-module.content.editTooltip' | translate"
                          (onClick)="beginEditAgent(agent)"></p-button>
                        <p-button
                          icon="pi pi-trash"
                          rounded="true"
                          severity="danger"
                          text="true"
                          tooltipPosition="bottom"
                          [disabled]="activeRowAction || !canDelete"
                          [pTooltip]="'agent-module.content.deleteTooltip' | translate"
                          (onClick)="deleteAgent(agent)"></p-button>
                      </div>
                    </td>
                  </tr>
                </ng-template>

                <ng-template pTemplate="emptymessage">
                  <tr>
                    <td colspan="7">{{ 'agent-module.agents.empty' | translate }}</td>
                  </tr>
                </ng-template>
              </p-table>
            </p-tabpanel>

            <p-tabpanel value="skills">
              <p-table dataKey="id" [paginator]="true" [rows]="50" [loading]="loading" [value]="visibleSkills">
                <ng-template pTemplate="header">
                  <tr>
                    <th pSortableColumn="code">
                      {{ 'agent-module.skills.code' | translate }}
                      <p-sortIcon field="code" />
                    </th>
                    <th>{{ 'agent-module.skills.description' | translate }}</th>
                    <th>{{ 'agent-module.skills.tools' | translate }}</th>
                    <th class="text-center">{{ 'agent-module.content.status' | translate }}</th>
                    <th pSortableColumn="updatedAt">
                      {{ 'agent-module.content.updatedAt' | translate }}
                      <p-sortIcon field="updatedAt" />
                    </th>
                    <th class="text-center min-w-32">{{ 'agent-module.content.actions' | translate }}</th>
                  </tr>
                </ng-template>

                <ng-template let-skill pTemplate="body">
                  <tr>
                    <td>
                      <span class="font-mono">{{ skill.code }}</span>
                    </td>
                    <td>{{ skill.description }}</td>
                    <td>
                      <div class="flex flex-wrap gap-1">
                        @for (tool of skill.tools ?? []; track tool) {
                          <p-tag severity="secondary" [value]="tool" />
                        }
                      </div>
                    </td>
                    <td class="text-center">
                      <p-tag
                        [severity]="skill.isEnabled ? 'success' : 'danger'"
                        [value]="
                          (skill.isEnabled ? 'agent-module.content.enabled' : 'agent-module.content.disabled')
                            | translate
                        " />
                    </td>
                    <td>{{ skill.updatedAt | date: layoutService.dateTimeFormat() }}</td>
                    <td>
                      <div class="flex items-center justify-center gap-1">
                        <p-button
                          icon="pi pi-pencil"
                          rounded="true"
                          text="true"
                          tooltipPosition="bottom"
                          [disabled]="activeRowAction || !canEdit"
                          [pTooltip]="'agent-module.content.editTooltip' | translate"
                          (onClick)="beginEditSkill(skill)"></p-button>
                        <p-button
                          icon="pi pi-trash"
                          rounded="true"
                          severity="danger"
                          text="true"
                          tooltipPosition="bottom"
                          [disabled]="activeRowAction || !canDelete"
                          [pTooltip]="'agent-module.content.deleteTooltip' | translate"
                          (onClick)="deleteSkill(skill)"></p-button>
                      </div>
                    </td>
                  </tr>
                </ng-template>

                <ng-template pTemplate="emptymessage">
                  <tr>
                    <td colspan="6">{{ 'agent-module.skills.empty' | translate }}</td>
                  </tr>
                </ng-template>
              </p-table>
            </p-tabpanel>

            <p-tabpanel value="tools">
              <p-table dataKey="name" [paginator]="true" [rows]="50" [loading]="loading" [value]="visibleTools">
                <ng-template pTemplate="header">
                  <tr>
                    <th pSortableColumn="name">
                      {{ 'agent-module.tools.name' | translate }}
                      <p-sortIcon field="name" />
                    </th>
                    <th>{{ 'agent-module.tools.description' | translate }}</th>
                    <th pSortableColumn="taskQueue">
                      {{ 'agent-module.tools.taskQueue' | translate }}
                      <p-sortIcon field="taskQueue" />
                    </th>
                    <th class="text-center">{{ 'agent-module.tools.requiresApproval' | translate }}</th>
                  </tr>
                </ng-template>

                <ng-template let-tool pTemplate="body">
                  <tr>
                    <td>
                      <span class="font-mono">{{ tool.name }}</span>
                    </td>
                    <td>{{ tool.description }}</td>
                    <td>{{ tool.taskQueue }}</td>
                    <td class="text-center">
                      @if (tool.requiresApproval) {
                        <p-tag severity="warn" [value]="'agent-module.tools.approval' | translate" />
                      }
                    </td>
                  </tr>
                </ng-template>

                <ng-template pTemplate="emptymessage">
                  <tr>
                    <td colspan="4">{{ 'agent-module.tools.empty' | translate }}</td>
                  </tr>
                </ng-template>
              </p-table>
            </p-tabpanel>
          </p-tabpanels>
        </p-tabs>
      </div>

      <p-dialog
        [header]="(editingAgentId ? 'agent-module.agentDialog.editTitle' : 'agent-module.agentDialog.createTitle') | translate"
        [modal]="true"
        [style]="{ width: '48rem' }"
        [(visible)]="agentDialogVisible">
        @if (agentModel; as model) {
          <div class="flex items-center gap-4 mb-4 mt-1">
            <label class="font-semibold w-1/3" for="oip-agent-module-agent-code">
              {{ 'agent-module.agents.code' | translate }}
            </label>
            <input
              autocomplete="off"
              class="flex-auto font-mono"
              id="oip-agent-module-agent-code"
              pInputText
              [placeholder]="'agent-module.agentDialog.codePlaceholder' | translate"
              [(ngModel)]="model.code" />
          </div>

          <div class="flex items-center gap-4 mb-4">
            <label class="font-semibold w-1/3" for="oip-agent-module-agent-name">
              {{ 'agent-module.agents.name' | translate }}
            </label>
            <input
              autocomplete="off"
              class="flex-auto"
              id="oip-agent-module-agent-name"
              pInputText
              [(ngModel)]="model.name" />
          </div>

          <div class="flex items-start gap-4 mb-4">
            <label class="font-semibold w-1/3 pt-2" for="oip-agent-module-agent-description">
              {{ 'agent-module.agents.description' | translate }}
            </label>
            <textarea
              class="flex-auto"
              id="oip-agent-module-agent-description"
              pTextarea
              rows="2"
              [autoResize]="true"
              [(ngModel)]="model.description"></textarea>
          </div>

          <div class="flex items-start gap-4 mb-4">
            <label class="font-semibold w-1/3 pt-2" for="oip-agent-module-agent-prompt">
              {{ 'agent-module.agents.systemPrompt' | translate }}
            </label>
            <textarea
              class="flex-auto"
              id="oip-agent-module-agent-prompt"
              pTextarea
              rows="6"
              [(ngModel)]="model.systemPrompt"></textarea>
          </div>

          <div class="flex items-center gap-4 mb-4">
            <label class="font-semibold w-1/3" for="oip-agent-module-agent-provider">
              {{ 'agent-module.agents.provider' | translate }}
            </label>
            <p-select
              appendTo="body"
              class="flex-auto"
              inputId="oip-agent-module-agent-provider"
              optionLabel="name"
              optionValue="id"
              [options]="providers"
              [placeholder]="'agent-module.agents.defaultProvider' | translate"
              [showClear]="true"
              [(ngModel)]="model.llmProviderId" />
          </div>

          <div class="flex items-center gap-4 mb-4">
            <label class="font-semibold w-1/3" for="oip-agent-module-agent-skills">
              {{ 'agent-module.agents.skills' | translate }}
            </label>
            <p-multiSelect
              appendTo="body"
              class="flex-auto"
              display="chip"
              inputId="oip-agent-module-agent-skills"
              optionLabel="code"
              optionValue="id"
              [filter]="skills.length > 10"
              [options]="skills"
              [(ngModel)]="model.skillIds" />
          </div>

          <div class="flex items-center gap-4 mb-4">
            <label class="font-semibold w-1/3" for="oip-agent-module-agent-enabled">
              {{ 'agent-module.content.enabled' | translate }}
            </label>
            <p-toggle-switch inputId="oip-agent-module-agent-enabled" [(ngModel)]="model.isEnabled" />
          </div>
        }

        <div class="flex justify-end gap-2">
          <p-button
            id="oip-agent-module-agent-cancel"
            severity="secondary"
            [disabled]="saving"
            [label]="'agent-module.dialog.cancel' | translate"
            (onClick)="agentDialogVisible = false" />
          <p-button
            id="oip-agent-module-agent-save"
            [disabled]="saving"
            [label]="'agent-module.dialog.save' | translate"
            [loading]="saving"
            (onClick)="saveAgent()" />
        </div>
      </p-dialog>

      <p-dialog
        [header]="(editingSkillId ? 'agent-module.skillDialog.editTitle' : 'agent-module.skillDialog.createTitle') | translate"
        [modal]="true"
        [style]="{ width: '48rem' }"
        [(visible)]="skillDialogVisible">
        @if (skillModel; as model) {
          <div class="flex items-center gap-4 mb-4 mt-1">
            <label class="font-semibold w-1/3" for="oip-agent-module-skill-code">
              {{ 'agent-module.skills.code' | translate }}
            </label>
            <input
              autocomplete="off"
              class="flex-auto font-mono"
              id="oip-agent-module-skill-code"
              pInputText
              [placeholder]="'agent-module.skillDialog.codePlaceholder' | translate"
              [(ngModel)]="model.code" />
          </div>

          <div class="flex items-start gap-4 mb-4">
            <label class="font-semibold w-1/3 pt-2" for="oip-agent-module-skill-description">
              {{ 'agent-module.skills.description' | translate }}
            </label>
            <textarea
              class="flex-auto"
              id="oip-agent-module-skill-description"
              pTextarea
              rows="2"
              [autoResize]="true"
              [placeholder]="'agent-module.skillDialog.descriptionPlaceholder' | translate"
              [(ngModel)]="model.description"></textarea>
          </div>

          <div class="flex items-start gap-4 mb-4">
            <label class="font-semibold w-1/3 pt-2" for="oip-agent-module-skill-instructions">
              {{ 'agent-module.skills.instructions' | translate }}
            </label>
            <textarea
              class="flex-auto"
              id="oip-agent-module-skill-instructions"
              pTextarea
              rows="10"
              [placeholder]="'agent-module.skillDialog.instructionsPlaceholder' | translate"
              [(ngModel)]="model.instructions"></textarea>
          </div>

          <div class="flex items-center gap-4 mb-4">
            <label class="font-semibold w-1/3" for="oip-agent-module-skill-tools">
              {{ 'agent-module.skills.tools' | translate }}
            </label>
            <p-multiSelect
              appendTo="body"
              class="flex-auto"
              display="chip"
              inputId="oip-agent-module-skill-tools"
              optionLabel="name"
              optionValue="name"
              [filter]="tools.length > 10"
              [options]="tools"
              [(ngModel)]="model.tools" />
          </div>

          <div class="flex items-center gap-4 mb-4">
            <label class="font-semibold w-1/3" for="oip-agent-module-skill-enabled">
              {{ 'agent-module.content.enabled' | translate }}
            </label>
            <p-toggle-switch inputId="oip-agent-module-skill-enabled" [(ngModel)]="model.isEnabled" />
          </div>
        }

        <div class="flex justify-end gap-2">
          <p-button
            id="oip-agent-module-skill-cancel"
            severity="secondary"
            [disabled]="saving"
            [label]="'agent-module.dialog.cancel' | translate"
            (onClick)="skillDialogVisible = false" />
          <p-button
            id="oip-agent-module-skill-save"
            [disabled]="saving"
            [label]="'agent-module.dialog.save' | translate"
            [loading]="saving"
            (onClick)="saveSkill()" />
        </div>
      </p-dialog>
    } @else if (isSettings) {
      <div class="flex flex-col md:flex-row gap-8">
        <div class="md:w-1/2">
          <div class="card flex flex-col gap-4">
            <div class="font-semibold text-xl">{{ 'agent-module.settings.title' | translate }}</div>
            <div class="grid grid-cols-12 gap-4">
              <label class="flex items-center col-span-12 mb-2 md:col-span-4 md:mb-0" for="showDisabled">
                {{ 'agent-module.settings.showDisabled' | translate }}
              </label>
              <div class="col-span-12 md:col-span-8">
                <p-toggle-switch inputId="showDisabled" [(ngModel)]="settings.showDisabled" />
              </div>
            </div>
            <div class="flex justify-end">
              <p-button
                icon="pi pi-save"
                [label]="'agent-module.settings.save' | translate"
                (onClick)="saveSettings(settings)"></p-button>
            </div>
          </div>
        </div>
      </div>
    } @else if (isSecurity) {
      <security [controller]="controller" [id]="id" />
    }
  `
})
export class AgentModuleComponent extends BaseModuleComponent<AgentModuleSettings, NoSettingsDto> {
  private readonly translations = provideTranslations({ en, ru });

  protected readonly dataService = inject(AgentModuleApi);
  private readonly confirmationService = inject(ConfirmationService);

  protected activeTab: AgentModuleTab = 'agents';
  protected agents: AgentDto[] = [];
  protected skills: SkillDto[] = [];
  protected tools: AgentToolDto[] = [];
  protected providers: LlmProviderDto[] = [];
  protected globalFilter = '';
  protected loading = false;
  protected saving = false;
  protected activeRowAction = false;

  protected agentDialogVisible = false;
  protected editingAgentId: number | null = null;
  protected agentModel: AgentEditModel | null = null;

  protected skillDialogVisible = false;
  protected editingSkillId: number | null = null;
  protected skillModel: SkillEditModel | null = null;

  constructor() {
    super();
    this.l10nService.get('agent-module').subscribe((l10n) => {
      this.appTitleService.setTitle(l10n.title);
    });
  }

  protected get visibleAgents(): AgentDto[] {
    const agents = this.settings?.showDisabled === false ? this.agents.filter((x) => x.isEnabled) : this.agents;
    return this.applyFilter(agents, (x) => [x.code, x.name, x.description]);
  }

  protected get visibleSkills(): SkillDto[] {
    const skills = this.settings?.showDisabled === false ? this.skills.filter((x) => x.isEnabled) : this.skills;
    return this.applyFilter(skills, (x) => [x.code, x.description, ...(x.tools ?? [])]);
  }

  protected get visibleTools(): AgentToolDto[] {
    return this.applyFilter(this.tools, (x) => [x.name, x.description, x.taskQueue]);
  }

  protected providerName(id: number | null | undefined): string {
    if (id === null || id === undefined) return String(this.t('agent-module.agents.defaultProvider'));
    return this.providers.find((x) => x.id === id)?.name ?? `#${id}`;
  }

  protected skillCodes(ids: number[] | null | undefined): string[] {
    return (ids ?? []).map((id) => this.skills.find((x) => x.id === id)?.code ?? `#${id}`);
  }

  protected override async onModuleInstanceChange(): Promise<void> {
    await this.loadAll();
  }

  protected async loadAll(): Promise<void> {
    if (this.securityRightsLoaded && !this.canRead) {
      this.agents = [];
      this.skills = [];
      this.tools = [];
      this.providers = [];
      return;
    }

    this.loading = true;
    try {
      [this.agents, this.skills, this.tools, this.providers] = await Promise.all([
        this.dataService.getAgents(),
        this.dataService.getSkills(),
        this.dataService.getTools(),
        this.dataService.getLlmProviders()
      ]);
    } catch (error) {
      this.msgService.errorFromException(error, String(this.t('agent-module.messages.loadError')));
    } finally {
      this.loading = false;
    }
  }

  protected beginCreateAgent(): void {
    if (!this.canEdit) return;
    this.editingAgentId = null;
    this.agentModel = {
      code: '',
      name: '',
      description: '',
      systemPrompt: '',
      llmProviderId: null,
      isEnabled: true,
      skillIds: []
    };
    this.agentDialogVisible = true;
  }

  protected beginEditAgent(agent: AgentDto): void {
    if (!this.canEdit || agent.id === undefined) return;
    this.editingAgentId = agent.id;
    this.agentModel = {
      code: agent.code ?? '',
      name: agent.name ?? '',
      description: agent.description ?? '',
      systemPrompt: agent.systemPrompt ?? '',
      llmProviderId: agent.llmProviderId ?? null,
      isEnabled: agent.isEnabled ?? true,
      skillIds: [...(agent.skillIds ?? [])]
    };
    this.agentDialogVisible = true;
  }

  protected async saveAgent(): Promise<void> {
    const model = this.agentModel;
    if (!model || !this.canEdit) return;

    const request: SaveAgentRequest = {
      code: model.code.trim(),
      name: model.name.trim(),
      description: model.description.trim() || null,
      systemPrompt: model.systemPrompt.trim() || null,
      llmProviderId: model.llmProviderId,
      isEnabled: model.isEnabled,
      skillIds: model.skillIds
    };

    if (!request.code || !request.name) {
      this.msgService.error(this.t('agent-module.messages.agentRequired'));
      return;
    }

    this.saving = true;
    try {
      if (this.editingAgentId === null) {
        await this.dataService.createAgent(request);
        this.msgService.success(this.t('agent-module.messages.createSuccess'));
      } else {
        await this.dataService.updateAgent({ id: this.editingAgentId }, request);
        this.msgService.success(this.t('agent-module.messages.updateSuccess'));
      }
      this.agentDialogVisible = false;
      await this.loadAll();
    } catch (error) {
      this.msgService.errorFromException(error, String(this.t('agent-module.messages.saveError')));
    } finally {
      this.saving = false;
    }
  }

  protected deleteAgent(agent: AgentDto): void {
    if (!this.canDelete || agent.id === undefined) return;
    const id = agent.id;
    this.confirmDelete(agent.name ?? agent.code ?? '', () => this.dataService.deleteAgent({ id }));
  }

  protected beginCreateSkill(): void {
    if (!this.canEdit) return;
    this.editingSkillId = null;
    this.skillModel = { code: '', description: '', instructions: '', isEnabled: true, tools: [] };
    this.skillDialogVisible = true;
  }

  protected beginEditSkill(skill: SkillDto): void {
    if (!this.canEdit || skill.id === undefined) return;
    this.editingSkillId = skill.id;
    this.skillModel = {
      code: skill.code ?? '',
      description: skill.description ?? '',
      instructions: skill.instructions ?? '',
      isEnabled: skill.isEnabled ?? true,
      tools: [...(skill.tools ?? [])]
    };
    this.skillDialogVisible = true;
  }

  protected async saveSkill(): Promise<void> {
    const model = this.skillModel;
    if (!model || !this.canEdit) return;

    const request: SaveSkillRequest = {
      code: model.code.trim(),
      description: model.description.trim(),
      instructions: model.instructions.trim(),
      isEnabled: model.isEnabled,
      tools: model.tools
    };

    if (!request.code || !request.description || !request.instructions) {
      this.msgService.error(this.t('agent-module.messages.skillRequired'));
      return;
    }

    this.saving = true;
    try {
      if (this.editingSkillId === null) {
        await this.dataService.createSkill(request);
        this.msgService.success(this.t('agent-module.messages.createSuccess'));
      } else {
        await this.dataService.updateSkill({ id: this.editingSkillId }, request);
        this.msgService.success(this.t('agent-module.messages.updateSuccess'));
      }
      this.skillDialogVisible = false;
      await this.loadAll();
    } catch (error) {
      this.msgService.errorFromException(error, String(this.t('agent-module.messages.saveError')));
    } finally {
      this.saving = false;
    }
  }

  protected deleteSkill(skill: SkillDto): void {
    if (!this.canDelete || skill.id === undefined) return;
    const id = skill.id;
    this.confirmDelete(skill.code ?? '', () => this.dataService.deleteSkill({ id }));
  }

  private confirmDelete(name: string, remove: () => Promise<unknown>): void {
    this.confirmationService.confirm({
      header: String(this.t('agent-module.confirm.header')),
      message: String(this.t('agent-module.confirm.message', { name })),
      icon: 'pi pi-trash',
      rejectButtonProps: {
        label: String(this.t('agent-module.confirm.cancel')),
        severity: 'secondary',
        outlined: true
      },
      acceptButtonProps: {
        label: String(this.t('agent-module.confirm.delete')),
        severity: 'danger'
      },
      accept: async () => {
        this.activeRowAction = true;
        try {
          await remove();
          this.msgService.success(this.t('agent-module.messages.deleteSuccess'));
          await this.loadAll();
        } catch (error) {
          this.msgService.errorFromException(error, String(this.t('agent-module.messages.deleteError')));
        } finally {
          this.activeRowAction = false;
        }
      }
    });
  }

  private applyFilter<T>(items: T[], fields: (item: T) => (string | null | undefined)[]): T[] {
    const filter = this.globalFilter.trim().toLocaleLowerCase();
    if (!filter) return items;
    return items.filter((item) => fields(item).some((x) => (x ?? '').toLocaleLowerCase().includes(filter)));
  }
}
