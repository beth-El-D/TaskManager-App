export const authGuard = () => {
  return !!localStorage.getItem('token');
};
