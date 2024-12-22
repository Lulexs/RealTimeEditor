import { User, UserLoginValues } from "../models/User";
import { makeAutoObservable, runInAction } from "mobx";
import { router } from "../routes/routes";

export default class UserStore {
  user: User | null = null;

  constructor() {
    makeAutoObservable(this);
  }

  get isLoggedIn() {
    return this.user != null;
  }

  login = async (value: UserLoginValues) => {
    runInAction(
      () =>
        (this.user = {
          region: "Serbia",
          username: value.username,
          avatar:
            "https://raw.githubusercontent.com/mantinedev/mantine/master/.demo/avatars/avatar-9.png",
        })
    );
    router.navigate("/nodocument");
  };

  register = async () => {};

  logout = () => {
    runInAction(() => (this.user = null));
    router.navigate("/");
  };
}
