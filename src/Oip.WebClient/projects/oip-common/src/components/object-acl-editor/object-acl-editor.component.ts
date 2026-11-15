import { Component, effect, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { AutoCompleteCompleteEvent, AutoCompleteModule, AutoCompleteSelectEvent } from 'primeng/autocomplete';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { TooltipModule } from 'primeng/tooltip';
import { AccessRightDto, AclEntryDto, ObjectAclDto, SubjectDto } from '../../dtos/object-acl.dto';
import { provideTranslations } from '../../helpers/l10n.helper';
import { MsgService } from '../../services/msg.service';
import { ObjectRightsService } from '../../services/object-rights.service';

import en from './l10n/object-acl-editor.en.json';
import ru from './l10n/object-acl-editor.ru.json';

type RightState = 'none' | 'allow' | 'deny';

interface EditableEntry extends SubjectDto {
  states: Record<string, RightState>;
}

const STANDARD_RIGHTS = ['read', 'edit', 'delete', 'manage-acl'];

/**
 * Editor of the access control list of one object: explicit rights per subject with allow and deny, the rights
 * inherited from the parent, and the inheritance switch.
 */
@Component({
  selector: 'oip-object-acl-editor',
  standalone: true,
  imports: [
    FormsModule,
    TranslatePipe,
    AutoCompleteModule,
    ButtonModule,
    TableModule,
    TagModule,
    ToggleSwitchModule,
    TooltipModule
  ],
  template: `
    @if (acl(); as acl) {
      <div class="flex flex-col gap-4">
        @if (acl.parentType) {
          <div class="flex items-center gap-3">
            <p-toggleswitch
              inputId="oip-acl-inherit"
              [ngModel]="!inheritanceBroken()"
              (ngModelChange)="inheritanceBroken.set(!$event)" />
            <label class="cursor-pointer" for="oip-acl-inherit">
              {{ 'objectAclEditor.inherit' | translate }}
            </label>
          </div>
        }

        <p-table [value]="entries()" dataKey="subjectKey" size="small">
          <ng-template pTemplate="header">
            <tr>
              <th>{{ 'objectAclEditor.subject' | translate }}</th>
              @for (right of acl.rights; track right.code) {
                <th class="text-center whitespace-nowrap">{{ rightName(right) }}</th>
              }
              <th class="w-12"></th>
            </tr>
          </ng-template>
          <ng-template let-entry pTemplate="body">
            <tr>
              <td>
                <div class="flex items-center gap-2">
                  <i [class]="subjectIcon(entry.subjectType)"></i>
                  <span>{{ subjectName(entry) }}</span>
                  @if (!entry.isKnown) {
                    <p-tag severity="warn" [value]="'objectAclEditor.unknown' | translate" />
                  }
                </div>
              </td>
              @for (right of acl.rights; track right.code) {
                <td class="text-center">
                  <p-button
                    rounded="true"
                    size="small"
                    text="true"
                    [attr.data-right]="right.code"
                    [attr.data-state]="entry.states[right.code]"
                    [icon]="stateIcon(entry.states[right.code])"
                    [pTooltip]="'objectAclEditor.state.' + entry.states[right.code] | translate"
                    [severity]="stateSeverity(entry.states[right.code])"
                    (onClick)="toggle(entry, right.code)" />
                </td>
              }
              <td class="text-center">
                <p-button
                  icon="pi pi-trash"
                  rounded="true"
                  severity="danger"
                  size="small"
                  text="true"
                  [pTooltip]="'objectAclEditor.remove' | translate"
                  (onClick)="remove(entry)" />
              </td>
            </tr>
          </ng-template>
          <ng-template pTemplate="emptymessage">
            <tr>
              <td class="text-surface-500" [attr.colspan]="acl.rights.length + 2">
                {{ 'objectAclEditor.noEntries' | translate }}
              </td>
            </tr>
          </ng-template>
        </p-table>

        <div class="flex flex-col md:flex-row md:items-center gap-2">
          <p-autoComplete
            appendTo="body"
            class="w-full md:w-96"
            inputId="oip-acl-subject-search"
            inputStyleClass="w-full"
            optionLabel="displayName"
            styleClass="w-full"
            [forceSelection]="true"
            [placeholder]="'objectAclEditor.addSubject' | translate"
            [suggestions]="suggestions()"
            [(ngModel)]="subjectQuery"
            (completeMethod)="search($event)"
            (onSelect)="add($event)">
            <ng-template let-subject pTemplate="item">
              <div class="flex items-center gap-2">
                <i [class]="subjectIcon(subject.subjectType)"></i>
                <span>{{ subjectName(subject) }}</span>
              </div>
            </ng-template>
          </p-autoComplete>
          <div class="flex-1"></div>
          <p-button
            icon="pi pi-save"
            id="oip-acl-save-button"
            [label]="'objectAclEditor.save' | translate"
            [loading]="saving()"
            (onClick)="save()" />
        </div>

        @if (!inheritanceBroken() && acl.inheritedEntries.length > 0) {
          <div class="flex flex-col gap-2">
            <div class="font-semibold">{{ 'objectAclEditor.inherited' | translate }}</div>
            <p-table [value]="acl.inheritedEntries" size="small">
              <ng-template let-entry pTemplate="body">
                <tr>
                  <td class="w-1/3">
                    <div class="flex items-center gap-2 text-surface-500">
                      <i [class]="subjectIcon(entry.subjectType)"></i>
                      <span>{{ subjectName(entry) }}</span>
                    </div>
                  </td>
                  <td>
                    <div class="flex flex-wrap gap-1">
                      @for (code of entry.allow; track code) {
                        <p-tag severity="success" [value]="rightNameByCode(code)" />
                      }
                      @for (code of entry.deny; track code) {
                        <p-tag severity="danger" [value]="'✕ ' + rightNameByCode(code)" />
                      }
                    </div>
                  </td>
                </tr>
              </ng-template>
            </p-table>
          </div>
        }
      </div>
    } @else if (loading()) {
      <div class="text-surface-500">{{ 'objectAclEditor.loading' | translate }}</div>
    }
  `
})
export class ObjectAclEditorComponent {
  private readonly translations = provideTranslations({ en, ru });
  private readonly rightsService = inject(ObjectRightsService);
  private readonly msgService = inject(MsgService);
  private readonly translateService = inject(TranslateService);
  private loadToken = 0;

  /** Route of the access control controller of the service, for example `customer-access-control`. */
  readonly apiBase = input.required<string>();
  readonly objectType = input.required<string>();
  readonly objectId = input.required<number>();
  /** Emitted after the access control list was saved. */
  readonly saved = output<void>();

  protected readonly acl = signal<ObjectAclDto | null>(null);
  protected readonly entries = signal<EditableEntry[]>([]);
  protected readonly inheritanceBroken = signal(false);
  protected readonly suggestions = signal<SubjectDto[]>([]);
  protected readonly loading = signal(false);
  protected readonly saving = signal(false);
  protected subjectQuery: SubjectDto | string | null = null;

  constructor() {
    effect(() => {
      void this.load(this.apiBase(), this.objectType(), this.objectId());
    });
  }

  protected rightName(right: AccessRightDto): string {
    return right.name || this.rightNameByCode(right.code);
  }

  protected rightNameByCode(code: string): string {
    const custom = this.acl()?.rights.find((r) => r.code === code)?.name;
    if (custom) return custom;
    return STANDARD_RIGHTS.includes(code) ? this.translateService.instant(`objectAclEditor.rights.${code}`) : code;
  }

  protected subjectName(subject: SubjectDto): string {
    if (subject.subjectType === 'everyone' || subject.subjectType === 'owner') {
      return this.translateService.instant(`objectAclEditor.subjects.${subject.subjectType}`);
    }
    return subject.displayName || subject.subjectKey;
  }

  protected subjectIcon(type: string): string {
    switch (type) {
      case 'role':
        return 'pi pi-id-card';
      case 'user':
        return 'pi pi-user';
      case 'client':
        return 'pi pi-server';
      case 'group':
        return 'pi pi-users';
      case 'owner':
        return 'pi pi-crown';
      default:
        return 'pi pi-globe';
    }
  }

  protected stateIcon(state: RightState): string {
    return state === 'allow' ? 'pi pi-check' : state === 'deny' ? 'pi pi-ban' : 'pi pi-minus';
  }

  protected stateSeverity(state: RightState): 'success' | 'danger' | 'secondary' {
    return state === 'allow' ? 'success' : state === 'deny' ? 'danger' : 'secondary';
  }

  protected toggle(entry: EditableEntry, code: string): void {
    const next: Record<RightState, RightState> = { none: 'allow', allow: 'deny', deny: 'none' };
    entry.states = { ...entry.states, [code]: next[entry.states[code] ?? 'none'] };
    this.entries.set([...this.entries()]);
  }

  protected remove(entry: EditableEntry): void {
    this.entries.set(this.entries().filter((e) => e !== entry));
  }

  protected async search(event: AutoCompleteCompleteEvent): Promise<void> {
    try {
      const subjects = await this.rightsService.searchSubjects(
        this.apiBase(),
        this.objectType(),
        this.objectId(),
        event.query
      );
      const taken = new Set(this.entries().map((e) => e.subjectKey));
      this.suggestions.set(subjects.filter((s) => !taken.has(s.subjectKey)));
    } catch (error) {
      this.suggestions.set([]);
      this.msgService.errorFromException(error, 'Subject search error');
    }
  }

  protected add(event: AutoCompleteSelectEvent): void {
    const subject = event.value as SubjectDto;
    if (!this.entries().some((e) => e.subjectKey === subject.subjectKey)) {
      this.entries.set([...this.entries(), this.toEditable({ ...subject, allow: [], deny: [] })]);
    }
    // The autocomplete writes the selected value to the model after this handler, so clear it afterwards.
    setTimeout(() => (this.subjectQuery = null));
  }

  protected async save(): Promise<void> {
    const acl = this.acl();
    if (!acl) return;

    this.saving.set(true);
    try {
      await this.rightsService.updateAcl(this.apiBase(), {
        objectType: acl.objectType,
        objectId: acl.objectId,
        inheritanceBroken: this.inheritanceBroken(),
        entries: this.entries().map((entry) => ({
          subjectKey: entry.subjectKey,
          allow: acl.rights.filter((r) => entry.states[r.code] === 'allow').map((r) => r.code),
          deny: acl.rights.filter((r) => entry.states[r.code] === 'deny').map((r) => r.code)
        }))
      });
      this.msgService.success(this.translateService.instant('objectAclEditor.saved'));
      this.saved.emit();
      await this.load(this.apiBase(), this.objectType(), this.objectId());
    } catch (error) {
      this.msgService.errorFromException(error, 'Save access control error');
    } finally {
      this.saving.set(false);
    }
  }

  private async load(apiBase: string, objectType: string, objectId: number): Promise<void> {
    const token = ++this.loadToken;
    this.loading.set(true);
    this.subjectQuery = null;
    this.suggestions.set([]);
    try {
      const acl = await this.rightsService.getAcl(apiBase, objectType, objectId);
      if (token !== this.loadToken) return;
      this.acl.set(acl);
      this.inheritanceBroken.set(acl.inheritanceBroken);
      this.entries.set(acl.entries.map((entry) => this.toEditable(entry)));
    } catch (error) {
      if (token !== this.loadToken) return;
      this.acl.set(null);
      this.msgService.errorFromException(error, 'Load access control error');
    } finally {
      if (token === this.loadToken) this.loading.set(false);
    }
  }

  private toEditable(entry: AclEntryDto): EditableEntry {
    const states: Record<string, RightState> = {};
    for (const right of this.acl()?.rights ?? []) {
      states[right.code] = entry.deny.includes(right.code)
        ? 'deny'
        : entry.allow.includes(right.code)
          ? 'allow'
          : 'none';
    }
    return { ...entry, states };
  }
}
