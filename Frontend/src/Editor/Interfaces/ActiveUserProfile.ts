import { UserProfile } from "./UserProfile";

export interface ActiveUserProfile extends UserProfile {
  userId: number;
}
