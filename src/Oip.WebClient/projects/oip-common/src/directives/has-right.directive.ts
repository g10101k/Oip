import { Directive, effect, inject, input, TemplateRef, ViewContainerRef } from '@angular/core';
import { ObjectRightsService } from '../services/object-rights.service';

/**
 * Renders the template only when the current user holds the right on an object.
 *
 * With rights already known, for example from a list row:
 * `*oipHasRight="'edit'; rights: customer.rights"`.
 *
 * Otherwise the rights are requested from the access control API of the service:
 * `*oipHasRight="'edit'; apiBase: 'customer-access-control'; objectType: 'demo.customer'; objectId: id"`.
 */
@Directive({
  selector: '[oipHasRight]',
  standalone: true
})
export class HasRightDirective {
  private readonly templateRef = inject(TemplateRef<unknown>);
  private readonly viewContainer = inject(ViewContainerRef);
  private readonly rightsService = inject(ObjectRightsService);
  private requestToken = 0;
  private rendered = false;

  readonly oipHasRight = input.required<string>();
  readonly oipHasRightRights = input<string[] | null | undefined>(undefined);
  readonly oipHasRightApiBase = input<string | undefined>(undefined);
  readonly oipHasRightObjectType = input<string | undefined>(undefined);
  readonly oipHasRightObjectId = input<number | null | undefined>(undefined);

  constructor() {
    effect(() => {
      const right = this.oipHasRight();
      const rights = this.oipHasRightRights();
      const apiBase = this.oipHasRightApiBase();
      const objectType = this.oipHasRightObjectType();
      const objectId = this.oipHasRightObjectId();
      const token = ++this.requestToken;

      if (rights) {
        this.render(rights.includes(right));
        return;
      }

      if (!apiBase || !objectType || objectId == null) {
        this.render(false);
        return;
      }

      this.rightsService.getRights(apiBase, objectType, objectId).then(
        (granted) => token === this.requestToken && this.render(granted.includes(right)),
        () => token === this.requestToken && this.render(false)
      );
    });
  }

  private render(visible: boolean): void {
    if (visible && !this.rendered) {
      this.viewContainer.createEmbeddedView(this.templateRef);
      this.rendered = true;
    } else if (!visible && this.rendered) {
      this.viewContainer.clear();
      this.rendered = false;
    }
  }
}
