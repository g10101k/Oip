import { SecurityDto } from './security.dto';

export interface PutSecurityDto {
  id: number;
  securities: SecurityDto[];
}
