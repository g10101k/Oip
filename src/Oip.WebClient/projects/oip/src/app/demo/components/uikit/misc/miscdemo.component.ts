import { Component, OnDestroy, OnInit } from '@angular/core';
import { ProgressBar } from 'primeng/progressbar';
import { Badge, BadgeDirective } from 'primeng/badge';
import { Button } from 'primeng/button';
import { AvatarGroup } from 'primeng/avatargroup';
import { Avatar } from 'primeng/avatar';
import { ScrollPanel } from 'primeng/scrollpanel';
import { ScrollTop } from 'primeng/scrolltop';
import { Tag } from 'primeng/tag';
import { Chip } from 'primeng/chip';
import { Skeleton } from 'primeng/skeleton';

@Component({
    templateUrl: './miscdemo.component.html',
    imports: [
        ProgressBar,
        Badge,
        BadgeDirective,
        Button,
        AvatarGroup,
        Avatar,
        ScrollPanel,
        ScrollTop,
        Tag,
        Chip,
        Skeleton,
    ],
})
export class MiscDemoComponent implements OnInit, OnDestroy {
  value = 0;
  interval: any;

  ngOnInit() {
    this.interval = setInterval(() => {
      this.value = this.value + Math.floor(Math.random() * 10) + 1;
      if (this.value >= 100) {
        this.value = 100;
        clearInterval(this.interval);
      }
    }, 2000);
  }

  ngOnDestroy() {
    clearInterval(this.interval);
  }

}
