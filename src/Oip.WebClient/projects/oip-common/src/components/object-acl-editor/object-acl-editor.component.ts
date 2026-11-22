import { Component, computed, effect, inject, input, output, signal } from '@angular/core';
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

/** Row of the editor: an explicit entry or, below them, a read-only entry inherited from the parent. */
interface AclRow {
  entry: EditableEntry;
  inherited: boolean;
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
      <div class="flex flex-col gap-3">
        <div class="flex flex-col md:flex-row md:items-center gap-3">
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
          <div class="hidden md:block flex-1"></div>
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
              <div class="flex items-center gap-2" [attr.data-subject-option]="subject.subjectKey">
                <i [class]="subjectIcon(subject.subjectType)"></i>
                <span>{{ subjectName(subject) }}</span>
              </div>
            </ng-template>
          </p-autoComplete>
        </div>

        <p-table size="small" [value]="rows()">
          <ng-template pTemplate="header">
            <tr>
              <th>{{ 'objectAclEditor.subject' | translate }}</th>
              @for (right of columns(); track right.code) {
                <th class="text-center whitespace-nowrap">
                  <span class="inline-flex items-center gap-1">
                    {{ rightName(right) }}
                    @if (right.description) {
                      <i class="pi pi-info-circle text-xs text-surface-400" [pTooltip]="right.description"></i>
                    }
                  </span>
                </th>
              }
              <th class="w-12"></th>
            </tr>
          </ng-template>
          <ng-template let-row pTemplate="body">
            @if (row.inherited) {
              <tr class="text-surface-500" [attr.data-inherited-subject]="row.entry.subjectKey">
                <td>
                  <div class="flex items-center gap-2">
                    <i class="pi pi-arrow-down-right text-xs"></i>
                    <i [class]="subjectIcon(row.entry.subjectType)"></i>
                    <span>{{ subjectName(row.entry) }}</span>
                    <p-tag severity="secondary" [value]="'objectAclEditor.inheritedTag' | translate" />
                  </div>
                </td>
                @for (right of columns(); track right.code) {
                  <td class="text-center">
                    <span
                      class="inline-flex items-center justify-center w-8 h-8 opacity-60"
                      [attr.data-inherited-right]="right.code"
                      [attr.data-state]="row.entry.states[right.code]"
                      [class]="stateColor(row.entry.states[right.code])"
                      [pTooltip]="'objectAclEditor.inheritedState.' + row.entry.states[right.code] | translate">
                      <i [class]="stateIcon(row.entry.states[right.code])"></i>
                    </span>
                  </td>
                }
                <td class="text-center">
                  @if (!isExplicit(row.entry.subjectKey)) {
                    <p-button
                      icon="pi pi-copy"
                      rounded="true"
                      severity="secondary"
                      size="small"
                      text="true"
                      [pTooltip]="'objectAclEditor.makeExplicit' | translate"
                      (onClick)="makeExplicit(row.entry)" />
                  }
                </td>
              </tr>
            } @else {
              <tr [attr.data-subject]="row.entry.subjectKey">
                <td>
                  <div class="flex items-center gap-2">
                    <i [class]="subjectIcon(row.entry.subjectType)"></i>
                    <span>{{ subjectName(row.entry) }}</span>
                    @if (!row.entry.isKnown) {
                      <p-tag severity="warn" [value]="'objectAclEditor.unknown' | translate" />
                    }
                  </div>
                </td>
                @for (right of columns(); track right.code) {
                  <td class="text-center">
                    <p-button
                      rounded="true"
                      size="small"
                      text="true"
                      [attr.data-right]="right.code"
                      [attr.data-state]="row.entry.states[right.code]"
                      [icon]="stateIcon(row.entry.states[right.code])"
                      [pTooltip]="'objectAclEditor.state.' + row.entry.states[right.code] | translate"
                      [severity]="stateSeverity(row.entry.states[right.code])"
                      (onClick)="toggle(row.entry, right.code)" />
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
                    (onClick)="remove(row.entry)" />
                </td>
              </tr>
            }
          </ng-template>
          <ng-template pTemplate="emptymessage">
            <tr>
              <td class="text-surface-500" [attr.colspan]="columns().length + 2">
                {{ 'objectAclEditor.noEntries' | translate }}
              </td>
            </tr>
          </ng-template>
        </p-table>

        <div class="flex flex-col md:flex-row md:items-center gap-3">
          <div class="flex flex-wrap items-center gap-x-4 gap-y-1 text-sm text-surface-500">
            <span class="inline-flex items-center gap-1">
              <i class="pi pi-check text-green-500"></i>{{ 'objectAclEditor.legend.allow' | translate }}
            </span>
            <span class="inline-flex items-center gap-1">
              <i class="pi pi-ban text-red-500"></i>{{ 'objectAclEditor.legend.deny' | translate }}
            </span>
            <span class="inline-flex items-center gap-1">
              <i class="pi pi-minus"></i>{{ 'objectAclEditor.legend.none' | translate }}
            </span>
            <span>{{ 'objectAclEditor.legend.hint' | translate }}</span>
          </div>
          @if (showSaveButton()) {
            <div class="flex-1"></div>
            <p-button
              icon="pi pi-save"
              id="oip-acl-save-button"
              [label]="'objectAclEditor.save' | translate"
              [loading]="saving()"
              (onClick)="save()" />
          }
        </div>
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
  /**
   * Rights to show, with names and hints, for example the rights a module declares. The manage-acl right is always
   * shown. When not set, every right of the object type is shown.
   */
  readonly rights = input<AccessRightDto[] | null | undefined>(undefined);
  /**
   * Shows the own save button. A host that saves the rights together with its own data hides it and calls
   * {@link save}.
   */
  readonly showSaveButton = input(true);
  /** Emitted after the access control list was saved. */
  readonly saved = output<void>();

  protected readonly acl = signal<ObjectAclDto | null>(null);
  protected readonly entries = signal<EditableEntry[]>([]);
  protected readonly inheritanceBroken = signal(false);
  protected readonly suggestions = signal<SubjectDto[]>([]);
  protected readonly loading = signal(false);
  protected readonly saving = signal(false);
  protected subjectQuery: SubjectDto | string | null = null;

  /** Columns of the editor: the rights to show that the object type knows. */
  protected readonly columns = computed<AccessRightDto[]>(() => {
    const known = this.acl()?.rights ?? [];
    const wanted = this.rights();
    if (!wanted) return known;
    const result = wanted.filter((right) => known.some((k) => k.code === right.code));
    if (!result.some((right) => right.code === 'manage-acl')) {
      const manageAcl = known.find((k) => k.code === 'manage-acl');
      if (manageAcl) result.push(manageAcl);
    }
    return result;
  });

  /** Explicit entries followed by the inherited ones, which are shown while the inheritance is on. */
  protected readonly rows = computed<AclRow[]>(() => {
    const explicit = this.entries().map((entry) => ({ entry, inherited: false }));
    const acl = this.acl();
    if (!acl || this.inheritanceBroken()) return explicit;
    const inherited = acl.inheritedEntries.map((entry) => ({ entry: this.toEditable(entry), inherited: true }));
    return [...explicit, ...inherited];
  });

  constructor() {
    effect(() => {
      void this.load(this.apiBase(), this.objectType(), this.objectId());
    });
  }

  protected rightName(right: AccessRightDto): string {
    return right.name || this.rightNameByCode(right.code);
  }

  protected visible(codes: string[]): string[] {
    const columns = this.columns();
    return codes.filter((code) => columns.some((c) => c.code === code));
  }

  protected rightNameByCode(code: string): string {
    const custom = this.columns().find((r) => r.code === code)?.name ?? this.acl()?.rights.find((r) => r.code === code)?.name;
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

  protected stateColor(state: RightState): string {
    return state === 'allow' ? 'text-green-500' : state === 'deny' ? 'text-red-500' : '';
  }

  protected stateSeverity(state: RightState): 'success' | 'danger' | 'secondary' {
    return state === 'allow' ? 'success' : state === 'deny' ? 'danger' : 'secondary';
  }

  protected toggle(entry: EditableEntry, code: string): void {
    const next: Record<RightState, RightState> = { none: 'allow', allow: 'deny', deny: 'none' };
    entry.states = { ...entry.states, [code]: next[entry.states[code] ?? 'none'] };
    this.entries.set([...this.entries()]);
  }

  protected isExplicit(subjectKey: string): boolean {
    return this.entries().some((e) => e.subjectKey === subjectKey);
  }

  /** Copies an inherited entry to the explicit ones, so it stays when the inheritance is switched off. */
  protected makeExplicit(entry: EditableEntry): void {
    if (this.isExplicit(entry.subjectKey)) return;
    this.entries.set([...this.entries(), { ...entry, states: { ...entry.states } }]);
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

  /**
   * Saves the access control list.
   * @returns true when saved, false when the list is not loaded or the server rejected it; the error is shown.
   */
  async save(): Promise<boolean> {
    const acl = this.acl();
    if (!acl) return false;

    this.saving.set(true);
    try {
      await this.rightsService.updateAcl(this.apiBase(), {
        objectType: acl.objectType,
        objectId: acl.objectId,
        inheritanceBroken: this.inheritanceBroken(),
        // Rights that are not shown keep their state, see toEditable.
        entries: this.entries().map((entry) => ({
          subjectKey: entry.subjectKey,
          allow: acl.rights.filter((r) => entry.states[r.code] === 'allow').map((r) => r.code),
          deny: acl.rights.filter((r) => entry.states[r.code] === 'deny').map((r) => r.code)
        }))
      });
      if (this.showSaveButton()) {
        this.msgService.success(this.translateService.instant('objectAclEditor.saved'));
      }
      this.saved.emit();
      await this.load(this.apiBase(), this.objectType(), this.objectId());
      return true;
    } catch (error) {
      this.msgService.errorFromException(error, 'Save access control error');
      return false;
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
